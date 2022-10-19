using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using EnergyStarX.Helpers;
using EnergyStarX.Services;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;

namespace EnergyStarX.ViewModels;

public partial class LogViewModel : ObservableRecipient
{
    private const int MaxLogCount = 500;

    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly WindowService windowService;

    [ObservableProperty]
    private ObservableCollection<string> logs = new();

    [ObservableProperty]
    private bool scrollToBottom = true;

    partial void OnScrollToBottomChanged(bool value) => ScrollToBottomIfNeeded();

    public event EventHandler? ScrollToBottomRequested;

    public LogViewModel(WindowService windowService)
    {
        this.windowService = windowService;
        
        StartDisplayingLog();

        // this.windowService.MainWindowShowing += (s, e) => StartDisplayingLog();
        // this.windowService.MainWindowHiding += (s, e) => StopDisplayingLog();
    }

    public void StartDisplayingLog()
    {
        Logger.NewLogLine -= Logger_NewLogLine;
        Logger.NewLogLine += Logger_NewLogLine;
    }

    public void StopDisplayingLog()
    {
        Logger.NewLogLine -= Logger_NewLogLine;
        Logs.Clear();
    }

    public void ScrollToBottomIfNeeded()
    {
        if (ScrollToBottom)
        {
            ScrollToBottomRequested?.Invoke(this, new EventArgs());
        }
    }

    [RelayCommand]
    private void CopyLogsToClipboard()
    {
        string allLogs = string.Join(Environment.NewLine, Logs);
        DataPackage dataPackage = new() { RequestedOperation = DataPackageOperation.Copy };
        dataPackage.SetText(allLogs);
        Clipboard.SetContent(dataPackage);
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
    }

    [RelayCommand]
    private async Task OpenLogFolder()
    {
        await Logger.OpenLogFolder();
    }

    private async void Logger_NewLogLine(object? sender, Logger.Message e)
    {
        await dispatcherQueue.EnqueueAsync(() =>
        {
            if (logs.Count >= MaxLogCount)
            {
                Logs.RemoveAt(0);
            }
            Logs.Add(e.LogString);
            ScrollToBottomIfNeeded();
        });
    }
}
