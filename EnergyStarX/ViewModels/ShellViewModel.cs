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

    public bool ShowTeachingTip
    {
        get => Settings.FirstRun;
        set => SetProperty(ShowTeachingTip, value, x => Settings.FirstRun = x);
    }

    public string TitlebarText { get; }
    public bool IsOsVersionNotRecommended { get; } = Environment.OSVersion.Version.Build < 22621;
    public string OsVersionNotRecommendedWarningMessage { get; } = string.Format("OsVersionNotRecommendedWarningMessage".GetLocalized(), Environment.OSVersion.Version.Build);

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        TitlebarText = HasAdminPrivilege() ?
            $"{"AppDisplayName".GetLocalized()}  ({"Admin Privilege".GetLocalized()})" :
            "AppDisplayName".GetLocalized();
    }

    [RelayCommand]
    private void CloseTeachingTip()
    {
        ShowTeachingTip = false;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            ShowTeachingTip = false;
            return;
        }

        NavigationViewItem? selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    private bool HasAdminPrivilege()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
