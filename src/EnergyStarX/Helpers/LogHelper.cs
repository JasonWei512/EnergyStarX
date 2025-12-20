using NLog;
using NLog.Layouts;
using Windows.Storage;
using Windows.System;

namespace EnergyStarX.Helpers;

public static class LogHelper
{
    private static readonly StorageFolder BaseFolder = ApplicationData.Current.LocalCacheFolder;

    private const string LogFolderName = "Log";
    private static readonly string LogFolderPath = Path.Combine(BaseFolder.Path, LogFolderName);

    private const string ErrorLogFileName = "Error.txt";
    private static readonly string ErrorLogFilePath = Path.Combine(LogFolderPath, ErrorLogFileName);

    private static readonly Layout InfoLogLayout = new SimpleLayout("${date} | ${level:uppercase=true} | ${message:withexception=true}");

    public static event EventHandler<Log>? NewLogLine;
    public record Log(string LogString, LogEventInfo LogEventInfo);

    /// <summary>
    /// For "Debug" log, log to debugger in debug build. <br/>
    /// For "Info" log, raise <see cref="NewLogLine" /> event and display it in LogPage. <br/>
    /// For "Error" log, log to file and Visual Studio App Center.
    /// </summary>
    public static void ConfigureNLog()
    {
        NLog.LogManager.Setup().LoadConfiguration(builder =>
        {
            builder.ForLogger()
                .FilterMinLevel(LogLevel.Debug)
                .WriteToDebugConditional();

            builder.ForLogger()
                .FilterMinLevel(LogLevel.Info)
                .WriteToMethodCall((logEventInfo, layouts) =>
                {
                    string logString = InfoLogLayout.Render(logEventInfo);
                    NewLogLine?.Invoke(null, new Log(logString, logEventInfo));
                });

            builder.ForLogger()
                .FilterMinLevel(LogLevel.Error)
                .WriteToFile(ErrorLogFilePath);
        });
    }

    public static async Task OpenLogFolder()
    {
        FolderLauncherOptions folderLauncherOptions = new();
        StorageFolder logFolder = await BaseFolder.CreateFolderAsync(LogFolderName, CreationCollisionOption.OpenIfExists);
        StorageFile errorLogFile = await logFolder.CreateFileAsync(ErrorLogFileName, CreationCollisionOption.OpenIfExists);
        folderLauncherOptions.ItemsToSelect.Add(errorLogFile);
        await Launcher.LaunchFolderAsync(logFolder, folderLauncherOptions);
    }

    public static async Task<string> GetErrorLogContent()
    {
        try
        {
            return await File.ReadAllTextAsync(ErrorLogFilePath);
        }
        catch
        {
            return string.Empty;
        }
    }
}
