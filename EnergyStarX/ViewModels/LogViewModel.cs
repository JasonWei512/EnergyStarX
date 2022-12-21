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
    private const int MaxLogCount = 100;

    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly WindowService windowService;

    public ObservableCollection<string> Logs { get; } = new();

    [ObservableProperty]
    private bool scrollToBottom = true;

    partial void OnScrollToBottomChanged(bool value) => ScrollToBottomIfNeeded();

    public event EventHandler? ScrollToBottomRequested;

    public LogViewModel(WindowService windowService)
    {
        this.windowService = windowService;
        this.windowService.WindowShowing += (s, e) => ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);

        StartDisplayingLog();
    }

    public void ScrollToBottomIfNeeded()
    {
        if (ScrollToBottom)
        {
            ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);
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
        await LogHelper.OpenLogFolder();
    }

    private void StartDisplayingLog()
    {
        LogHelper.NewLogLine -= Logger_NewLogLine;
        LogHelper.NewLogLine += Logger_NewLogLine;
    }

    private void StopDisplayingLog()
    {
        LogHelper.NewLogLine -= Logger_NewLogLine;
        Logs.Clear();
    }

    private async void Logger_NewLogLine(object? sender, LogHelper.Log e)
    {
        await dispatcherQueue.EnqueueAsync(() =>
        {
            if (Logs.Count >= MaxLogCount)
            {
                Logs.RemoveAt(0);
            }
            Logs.Add(e.LogString);
            ScrollToBottomIfNeeded();
        });
    }
}
