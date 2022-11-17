using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using NLog;
using NLog.Layouts;
using Windows.Storage;
using Windows.System;

namespace EnergyStarX.Helpers;

public static class LoggerHelper
{
    private static readonly StorageFolder BaseFolder = ApplicationData.Current.LocalCacheFolder;

    private const string LogFolderName = "Log";
    private static readonly string LogFolderPath = Path.Combine(BaseFolder.Path, LogFolderName);

    private const string ErrorLogFileName = "Error.txt";
    private static readonly string ErrorLogFilePath = Path.Combine(LogFolderPath, ErrorLogFileName);

    private static readonly SimpleLayout simpleLogLayout = new("${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}");

    public static event EventHandler<Log>? NewLogLine;
    public record Log(string LogString, LogEventInfo LogEventInfo);

    public static async Task OpenLogFolder()
    {
        FolderLauncherOptions folderLauncherOptions = new();
        StorageFolder logFolder = await BaseFolder.CreateFolderAsync(LogFolderName, CreationCollisionOption.OpenIfExists);
        StorageFile errorLogFile = await logFolder.CreateFileAsync(ErrorLogFileName, CreationCollisionOption.OpenIfExists);
        folderLauncherOptions.ItemsToSelect.Add(errorLogFile);
        await Launcher.LaunchFolderAsync(logFolder, folderLauncherOptions);
    }

    public static void ConfigureNLog()
    {
        NLog.LogManager.Setup().LoadConfiguration(builder =>
        {
            // For "Debug" log, log to debugger
            builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToDebugConditional();

            // For "Info" log, raise "NowLogLine" event and display it in LogPage
            builder.ForLogger().FilterMinLevel(LogLevel.Info)
                .WriteToMethodCall((logEventInfo, layouts) =>
                {
                    string logString = simpleLogLayout.Render(logEventInfo);
                    NewLogLine?.Invoke(null, new Log(logString, logEventInfo));
                });

            // For "Error" log, log to file and Visual Studio App Center
            builder.ForLogger().FilterMinLevel(LogLevel.Error)
                .WriteToFile(ErrorLogFilePath)
                .WriteToMethodCall((logEventInfo, layouts) =>
                {
                    // Send error info to App Center
                    Dictionary<string, string> appCenterProperties = new()
                    {
                        { "Time",  logEventInfo.TimeStamp.ToString() },
                        { "Message", logEventInfo.FormattedMessage },
                    };

                    if (logEventInfo.Exception is Exception exception)
                    {
                        if (exception.StackTrace is string stackTrace)
                        {
                            appCenterProperties.Add("StackTrace", stackTrace);
                        }
                        Crashes.TrackError(exception, appCenterProperties);
                    }
                    else
                    {
                        Analytics.TrackEvent("Error", appCenterProperties);
                    }
                });
        });
    }
}
