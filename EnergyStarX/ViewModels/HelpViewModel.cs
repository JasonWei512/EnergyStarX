using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Windows.System;

namespace EnergyStarX.ViewModels;

public partial class HelpViewModel : ObservableRecipient
{
    [RelayCommand]
    private async Task OpenLinkInBrowser(LinkClickedEventArgs e)
    {
        if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri? link))
        {
            await Launcher.LaunchUriAsync(link);
        }
    }
}
