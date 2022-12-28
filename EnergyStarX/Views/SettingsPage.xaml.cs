using EnergyStarX.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EnergyStarX.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel.ProcessWhitelistEditorDialogShowRequested += ViewModel_ProcessWhitelistEditorDialogShowRequested;
        ViewModel.ProcessBlacklistEditorDialogShowRequested += ViewModel_ProcessBlacklistEditorDialogShowRequested;
        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        ViewModel.ProcessWhitelistEditorDialogShowRequested -= ViewModel_ProcessWhitelistEditorDialogShowRequested;
        ViewModel.ProcessBlacklistEditorDialogShowRequested -= ViewModel_ProcessBlacklistEditorDialogShowRequested;
        base.OnNavigatingFrom(e);
    }

    private async void ViewModel_ProcessWhitelistEditorDialogShowRequested(object? sender, EventArgs e)
    {
        await ProcessWhitelistEditorDialog.ShowAsync();
    }

    private async void ViewModel_ProcessBlacklistEditorDialogShowRequested(object? sender, EventArgs e)
    {
        await ProcessBlacklistEditorDialog.ShowAsync();
    }

    private void ContentArea_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        MainContentArea.Width = Math.Min(ContentArea.ActualWidth, MainContentArea.MaxWidth);
    }
}
