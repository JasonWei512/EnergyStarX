using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnergyStarX.Contracts.Services;
using EnergyStarX.Helpers;
using EnergyStarX.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Security.Principal;

namespace EnergyStarX.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }

    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    [ObservableProperty]
    private bool showTeachingTip;

    public string TitlebarText { get; }
    public bool IsOsVersionNotRecommended { get; } = Environment.OSVersion.Version.Build < 22621;
    public string OsVersionNotRecommendedWarningMessage { get; } = string.Format("OsVersionNotRecommendedWarningMessage".ToLocalized(), Environment.OSVersion.Version.Build);

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        TitlebarText =
            "AppDisplayName".ToLocalized()
            + (HaveAdminPrivilege() ? $"  ({"Admin Privilege".ToLocalized()})" : string.Empty);
    }

    public async Task Initialize()
    {
        // A bug of Windows App SDK 1.2's TeachingTip:
        // If TeachingTip.Target is bound to a control, and TeachingTip.IsOpen is true, then the close button won't work.
        // The workaround is to add a delay before opening the TeachingTip after ShellPage is loaded.
        // https://github.com/microsoft/microsoft-ui-xaml/issues/7937#issuecomment-1382346727
        await Task.Delay(TimeSpan.FromSeconds(1));
        ShowTeachingTip = Settings.FirstRun;
    }

    [RelayCommand]
    private void CloseTeachingTip()
    {
        if (ShowTeachingTip)
        {
            ShowTeachingTip = false;
            Settings.FirstRun = false;
        }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            CloseTeachingTip();
            return;
        }

        NavigationViewItem? selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    private bool HaveAdminPrivilege()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
