using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using EnergyStarX.Helpers;
using EnergyStarX.Interfaces.Services;
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

    private readonly IEnergyService energyService;

    [ObservableProperty]
    private ImageSource? statusIcon;

    [ObservableProperty]
    private string? statusDescription;

    public bool PauseThrottling
    {
        get => energyService.PauseThrottling;
        set => SetProperty(PauseThrottling, value, x => energyService.PauseThrottling = x);
    }

    public HomeViewModel(IEnergyService energyService)
    {
        this.energyService = energyService;
        UpdateStatusOnUI(this.energyService.ThrottleStatus);

        this.energyService.ThrottleStatusChanged += EnergyService_ThrottleStatusChanged;
    }

    private void UpdateStatusOnUI(ThrottleStatus throttleStatus)
    {
        (StatusIcon, StatusDescription) = throttleStatus switch
        {
            ThrottleStatus.BlacklistAndAllButWhitelist => (ThrottlingIcon, ThrottlingDescription),
            ThrottleStatus.OnlyBlacklist => (NotThrottlingACIcon, NotThrottlingACDescription),
            ThrottleStatus.Stopped => (ThrottlingPausedIcon, ThrottlingPausedDescription),
            _ => throw new ArgumentException("Unknown ThrottleStatus")
        };
    }

    private async void EnergyService_ThrottleStatusChanged(object? sender, ThrottleStatus e)
    {
        await dispatcherQueue.EnqueueAsync(() => UpdateStatusOnUI(e));
    }
}
