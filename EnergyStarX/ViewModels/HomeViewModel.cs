using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using EnergyStarX.Helpers;
using EnergyStarX.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace EnergyStarX.ViewModels;

public partial class HomeViewModel : ObservableRecipient
{
    private readonly ImageSource ThrottlingIcon = new BitmapImage(new Uri("ms-appx:///Assets/InApp/CheckButton.png"));
    private readonly string ThrottlingDescription = "Home_Throttling_Description".ToLocalized();

    private readonly ImageSource NotThrottlingACIcon = new BitmapImage(new Uri("ms-appx:///Assets/InApp/PauseButton.png"));
    private readonly string NotThrottlingACDescription = "Home_NotThrottlingAC_Description".ToLocalized();

    private readonly ImageSource ThrottlingPausedIcon = new BitmapImage(new Uri("ms-appx:///Assets/InApp/PauseButton.png"));
    private readonly string ThrottlingPausedDescription = "Home_ThrottlingPaused_Description".ToLocalized();

    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly EnergyService energyService;

    [ObservableProperty]
    private ImageSource? statusIcon;

    [ObservableProperty]
    private string? statusDescription;

    public bool PauseThrottling
    {
        get => energyService.PauseThrottling;
        set => SetProperty(PauseThrottling, value, x => energyService.PauseThrottling = x);
    }

    public HomeViewModel(EnergyService energyService)
    {
        this.energyService = energyService;
        UpdateStatusOnUI(this.energyService.Status);

        this.energyService.StatusChanged += EnergyService_StatusChanged;
    }

    private void UpdateStatusOnUI(EnergyService.EnergyStatus energyStatus)
    {
        ThrottleStatus throttleStatus = energyService.ThrottleStatus;

        if (throttleStatus == ThrottleStatus.Stopped)
        {
            StatusIcon = ThrottlingPausedIcon;
            StatusDescription = ThrottlingPausedDescription;
        }
        else if (throttleStatus == ThrottleStatus.OnlyBlacklist)
        {
            StatusIcon = NotThrottlingACIcon;
            StatusDescription = NotThrottlingACDescription;
        }
        else if (throttleStatus == ThrottleStatus.BlacklistAndAllButWhitelist)
        {
            StatusIcon = ThrottlingIcon;
            StatusDescription = ThrottlingDescription;
        }
    }

    private async void EnergyService_StatusChanged(object? sender, EnergyService.EnergyStatus e)
    {
        await dispatcherQueue.EnqueueAsync(() => UpdateStatusOnUI(e));
    }
}
