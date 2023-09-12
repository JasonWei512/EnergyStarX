using EnergyStarX.Contracts.Services;
using Microsoft.AppCenter.Analytics;
using Microsoft.UI.Xaml;

namespace EnergyStarX.Services;

public class WindowService : IWindowService
{
    public bool WindowVisible => App.MainWindow.Visible;

    public event EventHandler? WindowShowing;
    public event EventHandler? WindowHiding;
    public event EventHandler? AppExiting;

    public void Initialize()
    {
        // Hook "Closed" event
        App.MainWindow.Closed += MainWindow_Closed;
    }

    public void ShowAppWindow()
    {
        if (!WindowVisible)
        {
            WindowShowing?.Invoke(this, EventArgs.Empty);
            Analytics.TrackEvent("Show app window");
        }
        App.MainWindow.Activate();
        App.MainWindow.BringToFront();
    }

    public void HideAppWindow()
    {
        if (WindowVisible)
        {
            WindowHiding?.Invoke(this, EventArgs.Empty);
        }
        App.MainWindow.Hide();
    }

    public void ExitApp()
    {
        App.MainWindow.Closed -= MainWindow_Closed;
        AppExiting?.Invoke(this, EventArgs.Empty);
        App.MainWindow.Close();
        Application.Current.Exit();
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        args.Handled = true;
        HideAppWindow();
    }
}
