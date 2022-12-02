using EnergyStarX.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace EnergyStarX.Services;

public class DialogService
{
    public async Task<bool> ShowConfirmationDialog(string title, string? content = null)
    {
        ContentDialog contentDialog = new()
        {
            Title = title,
            Content = content,

            PrimaryButtonText = "Yes".ToLocalized(),
            SecondaryButtonText = "No".ToLocalized(),
            DefaultButton = ContentDialogButton.Secondary,

            XamlRoot = App.MainWindow.Content.XamlRoot,
            RequestedTheme = ElementTheme.Default
        };

        ContentDialogResult result = await contentDialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}
