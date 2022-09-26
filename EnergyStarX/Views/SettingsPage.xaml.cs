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
        ViewModel.BypassProcessListEditorDialogShowRequested += ViewModel_BypassProcessListEditorDialogShowRequested;
        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        ViewModel.BypassProcessListEditorDialogShowRequested -= ViewModel_BypassProcessListEditorDialogShowRequested;
        base.OnNavigatingFrom(e);
    }

    private async void ViewModel_BypassProcessListEditorDialogShowRequested(object? sender, EventArgs e)
    {
        await BypassProcessListEditorDialog.ShowAsync();
    }

    private void ContentArea_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        MainContentArea.Width = Math.Min(ContentArea.ActualWidth, MainContentArea.MaxWidth);
    }
}
