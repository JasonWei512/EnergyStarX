using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Windows.Storage;
using Windows.System;

namespace EnergyStarX.Helpers;

public static class Logger
{
    private static readonly StorageFolder BaseFolder = ApplicationData.Current.LocalCacheFolder;
    private const string LogFolderName = "Log";
    private const string LogFileName = "Log.txt";

    private static readonly SemaphoreSlim semaphore = new(1);

    public static event EventHandler<Message>? NewLogLine;
    public record Message(DateTime Time, string Value, string LogString);

    public static void Debug(string message)
    {
#if DEBUG
        Log("DEBUG", message);
#endif
    }

    public static void Info(string message) => Log("INFO ", message);

    public static void Error(string message, Exception? exception = null)
    {
        if (exception is not null)
        {
            message +=
                $"""

                Exception:
                {exception}
                """;
        }

        // Send error info to App Center
        Dictionary<string, string> appCenterProperties = new()
        {
            { "Time",  DateTime.Now.ToString() },
            { "Message", message },
        };

        if (exception is not null)
        {
            Crashes.TrackError(exception, appCenterProperties);
        }
        else
        {
            Analytics.TrackEvent("Error", appCenterProperties);
        }

        Log("ERROR", message, true);
    }

    public static async Task OpenLogFolder()
    {
        FolderLauncherOptions folderLauncherOptions = new();
        StorageFolder logFolder = await GetLogFolder();
        StorageFile logFile = await GetLogFile();
        folderLauncherOptions.ItemsToSelect.Add(logFile);
        await Launcher.LaunchFolderAsync(logFolder, folderLauncherOptions);
    }

    private static void Log(string logLevel, string message, bool logToFile = false)
    {
        DateTime time = DateTime.Now;
        string logString = GetLogString(time, logLevel, message);

        System.Diagnostics.Debug.WriteLine(logString);

        NewLogLine?.Invoke(null, new Message(time, message, logString));

        if (logToFile)
        {
            _ = AppendLogToFile(logString);
        }
    }

    private static string GetLogString(DateTime time, string logLevel, string message)
    {
        string prefix = $"{time} - {logLevel} | ";
        string logString = $"{prefix}{message.Replace("\n", $"\n{new string(' ', prefix.Length)}")}";

        return logString;
    }

    private static async Task AppendLogToFile(string message)
    {
        await semaphore.WaitAsync();
        try
        {
            StorageFile logFile = await GetLogFile();
            await FileIO.AppendLinesAsync(logFile, new[] { message });
        }
        catch { }
        finally
        {
            semaphore.Release();
        }
    }

    private static async Task<StorageFolder> GetLogFolder()
    {
        return await BaseFolder.CreateFolderAsync(LogFolderName, CreationCollisionOption.OpenIfExists);
    }

    private static async Task<StorageFile> GetLogFile()
    {
        StorageFolder logFolder = await GetLogFolder();
        return await logFolder.CreateFileAsync(LogFileName, CreationCollisionOption.OpenIfExists);
    }
}
