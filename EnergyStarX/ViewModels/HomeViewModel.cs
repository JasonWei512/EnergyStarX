using CommunityToolkit.Mvvm.ComponentModel;
using EnergyStarX.Helpers;
using EnergyStarX.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.System.Power;

namespace EnergyStarX.ViewModels;

public partial class HomeViewModel : ObservableRecipient
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly EnergyService energyService;

    [ObservableProperty]
    private string? description;

    [ObservableProperty]
    private ImageSource? statusIconSource;

    public HomeViewModel(EnergyService energyService)
    {
        this.energyService = energyService;
        UpdateStatusOnUi(this.energyService.Status);

        this.energyService.StatusChanged += EnergyService_StatusChanged;
    }

    private void UpdateStatusOnUi(EnergyService.EnergyStatus energyStatus)
    {
        if (energyStatus.IsThrottling)
        {
            StatusIconSource = new BitmapImage(new Uri("ms-appx:///Assets/InApp/CheckButton.png"));
            Description = "Home_Throttling_Description".GetLocalized();
        }
        else if (energyStatus.PowerSourceKind == PowerSourceKind.AC)
        {
            StatusIconSource = new BitmapImage(new Uri("ms-appx:///Assets/InApp/PauseButton.png"));
            Description = "Home_NotThrottlingAC_Description".GetLocalized();
        }
    }

    private async void EnergyService_StatusChanged(object? sender, EnergyService.EnergyStatus e)
    {
        await CommunityToolkit.WinUI.DispatcherQueueExtensions.EnqueueAsync(dispatcherQueue, () =>
        {
            UpdateStatusOnUi(e);
        });
    }
}
