using EnergyStarX.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EnergyStarX.Views;

public sealed partial class LogPage : Page
{
    public LogViewModel ViewModel { get; }

    public LogPage()
    {
        ViewModel = App.GetService<LogViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel.ScrollToBottomRequested += ViewModel_ScrollToBottomRequested;
        ViewModel.ScrollToBottomIfNeeded();
        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        ViewModel.ScrollToBottomRequested -= ViewModel_ScrollToBottomRequested;
        base.OnNavigatingFrom(e);
    }

    private async void ViewModel_ScrollToBottomRequested(object? sender, object e)
    {
        await ScrollToBottom();
    }

    private async Task ScrollToBottom()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        LogScrollViewer.ChangeView(null, double.MaxValue, null, true);
    }
}