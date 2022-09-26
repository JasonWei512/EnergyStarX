using EnergyStarX.Helpers;
using Microsoft.UI.Xaml;

namespace EnergyStarX.Services;

public class WindowService
{
    private readonly EnergyService energyService;

    public event EventHandler? MainWindowShowing;
    public event EventHandler? MainWindowHiding;
    public event EventHandler? AppExiting;

    public WindowService(EnergyService energyService)
    {
        this.energyService = energyService;
    }

    public void Initialize()
    {
        // Hook "Closed" event
        App.MainWindow.Closed += MainWindow_Closed;
    }

    public void ShowAppWindow()
    {
        MainWindowShowing?.Invoke(this, new EventArgs());
        App.MainWindow.Activate();
        App.MainWindow.BringToFront();
        App.MainWindow.Backdrop = new MicaSystemBackdropEx(); // Workaround for https://github.com/dotMorten/WinUIEx/issues/55
    }

    public void HideAppWindow()
    {
        MainWindowHiding?.Invoke(this, new EventArgs());
        App.MainWindow.Hide();
    }

    public void ExitApp()
    {
        energyService.Terminate();

        App.MainWindow.Closed -= MainWindow_Closed;
        AppExiting?.Invoke(this, new EventArgs());
        App.MainWindow.Close();
        Application.Current.Exit();
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        args.Handled = true;
        HideAppWindow();
    }
}
