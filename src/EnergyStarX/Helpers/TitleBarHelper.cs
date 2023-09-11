using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Windows.UI.ViewManagement;

namespace EnergyStarX.Helpers;

// Helper class to workaround custom title bar bugs.
// DISCLAIMER: The resource key names and color values used below are subject to change. Do not depend on them.
// https://github.com/microsoft/TemplateStudio/issues/4516
public static class TitleBarHelper
{
    private static readonly UISettings uiSettings = new();
    private static readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    /// <summary>
    /// Listen to Windows theme changed event, and update title bar button color when Windows theme changes.
    /// </summary>
    public static void UpdateButtonColorWhenWindowsThemeChanges(this AppWindowTitleBar titleBar)
    {
        UpdateTitleBarButtonColor(titleBar);

        uiSettings.ColorValuesChanged += (s, e) =>
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                UpdateTitleBarButtonColor(titleBar);
            });
        };
    }

    /// <summary>
    /// Manually triggers title bar button color update.
    /// </summary>
    private static void UpdateTitleBarButtonColor(AppWindowTitleBar titleBar)
    {
        if (titleBar.ExtendsContentIntoTitleBar)
        {
            titleBar.ForegroundColor = null;
        }
    }
}