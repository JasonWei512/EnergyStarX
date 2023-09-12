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
    private readonly AppInstance currentAppInstance = AppInstance.GetCurrent();

    private readonly ISystemTrayIconService systemTrayIconService;
    private readonly IWindowService windowService;
    private readonly IEnergyService energyService;
    private readonly IStartupService startupService;

    public ActivationService(
        ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        IWindowService windowService,
        IEnergyService energyService,
        LogViewModel logViewModel,
        ISystemTrayIconService systemTrayIconService,
        IStartupService startupService
        )
    {
        this.defaultHandler = defaultHandler;
        this.activationHandlers = activationHandlers;
        this.windowService = windowService;
        this.energyService = energyService;
        this.systemTrayIconService = systemTrayIconService;
        this.startupService = startupService;
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

    /// <summary>
    /// Execute tasks before activation.
    /// </summary>
    private async Task Initialize()
    {
        await startupService.Initialize();
    }

    /// <summary>
    /// Execute tasks after activation.
    /// </summary>
    private async Task Startup()
    {
        windowService.Initialize();
        energyService.Initialize();
        await systemTrayIconService.Initialize();
    }

    private bool ShouldShowAppWindow()
    {
        // If app is run at startup, don't show app window during activation
        if (currentAppInstance.GetActivatedEventArgs().Kind == ExtendedActivationKind.StartupTask)
        {
            return false;
        }

        // If CLI Args contains one of these, don't show app window during activation
        string[] silentStartArgs = { "-s", "--silent" };
        if (Environment.GetCommandLineArgs().Any(arg => silentStartArgs.Contains(arg)))
        {
            return false;
        }

        return true;
    }
}
