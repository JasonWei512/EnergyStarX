using Microsoft.UI.Xaml;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace EnergyStarX.Helpers;

public static class ThemeHelper
{
    private static readonly UISettings uISettings = new();

    public static ElementTheme SystemTheme
    {
        get
        {
            Color color = uISettings.GetColorValue(UIColorType.Background); // System background color

            return (color.R, color.G, color.B) switch
            {
                (0, 0, 0) => ElementTheme.Dark,
                (255, 255, 255) => ElementTheme.Light,
                _ => throw new ArgumentException("Unknown system theme")
            };
        }
    }
}
