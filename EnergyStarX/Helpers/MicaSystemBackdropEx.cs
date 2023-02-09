using Microsoft.UI.Composition.SystemBackdrops;

namespace EnergyStarX.Helpers;

/// <summary>
/// Defines the Mica System Backdrop settings to apply to the window. <br/>
/// It's a modified version of <see cref="MicaSystemBackdrop"/> from <see cref="WinUIEx"/>, 
/// with the <see cref="UpdateController"/> method modified for fixing a wrong Mica theme bug. <br/>
/// <br/>
/// Source: <see href="https://github.com/dotMorten/WinUIEx/blob/a2d8a2a7044d31f4780f535986e860361729484d/src/WinUIEx/SystemBackdrop.cs#L249"/>
/// </summary>
/// <seealso cref="MicaController"/>
public class MicaSystemBackdropEx : MicaSystemBackdrop
{
    protected override void UpdateController(ISystemBackdropController controller, SystemBackdropTheme theme)
    {
        // Below is a workaround for the wrong Mica theme bug

        theme = ThemeHelper.SystemTheme switch
        {
            SystemTheme.Light => SystemBackdropTheme.Light,
            SystemTheme.Dark => SystemBackdropTheme.Dark,
            _ => throw new ArgumentException("Unknown system theme")
        };

        /* Explanation:
         * 
         * The parameter "SystemBackdropTheme theme" can be wrong, which may cause incorrect Mica theme.
         * Here's how to reproduce the bug:
         * 
         * - Set your MainWindow's backdrop to WinUIEx's MicaSystemBackdrop
         * - Set your Windows system theme to dark
         * - Hide MainWindow in MainWindow.Close event, like in https://github.com/dotMorten/WinUIEx/issues/55#issuecomment-1211503744
         * - Close the MainWindow to hide it
         * - Set Windows system theme to light
         * - Show the MainWindow
         * - Set MainWindow.Backdrop to "new MicaSystemBackdrop()" (A workaround for https://github.com/dotMorten/WinUIEx/issues/55#issuecomment-1207679616)
         * 
         * You can see the Mica theme is still dark, because the parameter "theme" is "SystemBackdropTheme.Dark".
         */

        base.UpdateController(controller, theme);
    }
}
