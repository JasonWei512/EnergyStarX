using CommunityToolkit.WinUI;
using EnergyStarX.Activation;
using EnergyStarX.Contracts.Services;
using EnergyStarX.ViewModels;
using EnergyStarX.Views;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;

namespace EnergyStarX.Services;

public class ActivationService : IActivationService
{
    private readonly AppInstance currentAppInstance = AppInstance.GetCurrent();
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly ActivationHandler<LaunchActivatedEventArgs> defaultHandler;
    private readonly IEnumerable<IActivationHandler> activationHandlers;
    private UIElement? shell = null;

    private readonly SystemTrayIconService systemTrayIconService;
    private readonly WindowService windowService;
    private readonly EnergyService energyService;
    private readonly StartupService startupService;

    public ActivationService(
        ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        WindowService windowService,
        EnergyService energyService,
        LogViewModel logViewModel,
        SystemTrayIconService systemTrayIconService,
        StartupService startupService
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

        currentAppInstance.Activated += CurrentAppInstance_Activated;
    }

    private async void CurrentAppInstance_Activated(object? sender, AppActivationArguments e)
    {
        // Mimic UWP's single instance app mode
        // When launching a second app instance, redirect activation to the main app instance (see Program.cs), and show main instance's app window
        await dispatcherQueue.EnqueueAsync(() => windowService.ShowAppWindow());
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
