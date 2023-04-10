using Windows.UI;
using Windows.UI.ViewManagement;

namespace EnergyStarX.Helpers;

public enum SystemTheme
{
    Light,
    Dark
};

public static class ThemeHelper
{
    private static readonly UISettings uiSettings = new();

    public static SystemTheme SystemTheme
    {
        get
        {
            Color color = uiSettings.GetColorValue(UIColorType.Background); // System background color

            return (color.R, color.G, color.B) switch
            {
                (0, 0, 0) => SystemTheme.Dark,
                (255, 255, 255) => SystemTheme.Light,
                _ => throw new ArgumentException("Unknown system theme")
            };
        }
    }
}
