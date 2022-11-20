using EnergyStarX.Activation;
using EnergyStarX.Contracts.Services;
using EnergyStarX.ViewModels;
using EnergyStarX.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;

namespace EnergyStarX.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> defaultHandler;
    private readonly IEnumerable<IActivationHandler> activationHandlers;
    private UIElement? shell = null;

    private readonly SystemTrayIconService systemTrayIconService;
    private readonly WindowService windowService;
    private readonly EnergyService energyService;

    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers, WindowService windowService, EnergyService energyService, LogViewModel logViewModel, SystemTrayIconService systemTrayIconService)
    {
        this.defaultHandler = defaultHandler;
        this.activationHandlers = activationHandlers;
        this.windowService = windowService;
        this.energyService = energyService;
        this.systemTrayIconService = systemTrayIconService;
    }

    public async Task Activate(object activationArgs)
    {
        // Execute tasks before activation.
        await Initialize();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            shell = App.GetService<ShellPage>();
            App.MainWindow.Content = shell ?? new Frame();
        }

        // Handle activation via ActivationHandlers.
        await HandleActivation(activationArgs);

        // Activate the MainWindow.
        if (ShouldShowAppWindow())
        {
            windowService.ShowAppWindow();
        }

        // Execute tasks after activation.
        await Startup();
    }

    private async Task HandleActivation(object activationArgs)
    {
        IActivationHandler? activationHandler = activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.Handle(activationArgs);
        }

        if (defaultHandler.CanHandle(activationArgs))
        {
            await defaultHandler.Handle(activationArgs);
        }
    }

    private async Task Initialize()
    {
        await Task.CompletedTask;
    }

    private async Task Startup()
    {
        windowService.Initialize();
        energyService.Initialize();
        await systemTrayIconService.Initialize();
    }

    private bool ShouldShowAppWindow()
    {
        // If app is run at startup, don't show app window during activation
        if (AppInstance.GetCurrent().GetActivatedEventArgs().Kind == ExtendedActivationKind.StartupTask)
        {
            return false;
        }

        // If CLI Args contains one of these, don't show app window during activation
        string[] silentStartArgs = { "-s", "--silent" };
        if (Environment.GetCommandLineArgs().Any(arg => InStringsIgnoreCase(arg, silentStartArgs)))
        {
            return false;
        }

        return true;
    }

    private bool InStringsIgnoreCase(string target, IEnumerable<string> stringCollection)
        => stringCollection.Any(s => string.Equals(s, target, StringComparison.InvariantCultureIgnoreCase));
}
