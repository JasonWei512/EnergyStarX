using CommunityToolkit.WinUI;
using EnergyStarX.Helpers;
using EnergyStarX.Interfaces.Services;
using H.NotifyIcon.Core;
using Microsoft.UI.Dispatching;
using NLog;
using Windows.ApplicationModel;

namespace EnergyStarX.Services;

public class SystemTrayIconService : ISystemTrayIconService
{
    private readonly System.Drawing.Icon ThrottlingIcon = new(Path.Combine(Package.Current.InstalledPath, "Assets/WindowIcon.ico"));
    private readonly string ThrottlingToolTip = "AppDisplayName".ToLocalized();

    private readonly System.Drawing.Icon NotThrottlingIcon = new(Path.Combine(Package.Current.InstalledPath, "Assets/WindowIcon-Gray.ico"));
    private readonly string NotThrottlingToolTip = $"{"AppDisplayName".ToLocalized()}\n({"Paused".ToLocalized()})";

    private readonly TrayIconWithContextMenu trayIcon = new();

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly IWindowService windowService;
    private readonly IEnergyService energyService;

    public SystemTrayIconService(IWindowService windowService, IEnergyService energyService)
    {
        this.windowService = windowService;
        this.energyService = energyService;
    }

    public async Task Initialize()
    {
        UpdateTrayIconImageAndToolTip(energyService.ThrottleStatus);

        trayIcon.ContextMenu = new PopupMenu()
        {
            Items =
            {
                new PopupMenuItem("Open".ToLocalized(), async (s, e) => await dispatcherQueue.EnqueueAsync(() => windowService.ShowAppWindow())),
                new PopupMenuItem("Exit".ToLocalized(), async (s, e) => await dispatcherQueue.EnqueueAsync(() => windowService.ExitApp()))
            }
        };

        trayIcon.MessageWindow.MouseEventReceived += async (s, e) =>
        {
            if (e.MouseEvent == MouseEvent.IconLeftMouseDown)
            {
                await dispatcherQueue.EnqueueAsync(() => windowService.ShowAppWindow());
            }
        };

        // When taskbar restarts (for example when explorer crashes and restarts), recreate system tray icon.
        // Or the system tray icon will disappear.
        trayIcon.MessageWindow.TaskbarCreated += (s, e) =>
        {
            logger.Info("Taskbar restarted. Recreating system tray icon...");

            try
            {
                trayIcon.TryRemove();
                trayIcon.Create();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to recreate system tray icon");
            }
        };

        trayIcon.Create();

        windowService.AppExiting += WindowService_AppExiting;
        energyService.ThrottleStatusChanged += EnergyService_ThrottleStatusChanged;

        // If user is using the taskbar enhancement tool StartAllBack, at this line the tray icon image may be wrong.
        // So I have to wait 0.1 second and update tray icon image and tooltip again.
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        UpdateTrayIconImageAndToolTip(energyService.ThrottleStatus);
    }

    private void WindowService_AppExiting(object? sender, EventArgs e)
    {
        trayIcon.Remove();
        trayIcon.Dispose();

        ThrottlingIcon.Dispose();
        NotThrottlingIcon.Dispose();
    }

    private async void EnergyService_ThrottleStatusChanged(object? sender, ThrottleStatus e)
    {
        await dispatcherQueue.EnqueueAsync(() => UpdateTrayIconImageAndToolTip(e));
    }

    private void UpdateTrayIconImageAndToolTip(ThrottleStatus throttleStatus)
    {
        (System.Drawing.Icon icon, string toolTip) = throttleStatus switch
        {
            ThrottleStatus.BlacklistAndAllButWhitelist => (ThrottlingIcon, ThrottlingToolTip),
            _ => (NotThrottlingIcon, NotThrottlingToolTip)
        };

        // Warning:
        // "trayIcon.UpdateIcon()" and "trayIcon.UpdateToolTip()" might throw "InvalidOperationException" when PC wakes up from sleep
        // https://github.com/HavenDV/H.NotifyIcon/issues/50
        // https://github.com/JasonWei512/EnergyStarX/issues/11

        try
        {
            trayIcon.UpdateIcon(icon.Handle);
        }
        catch (Exception e)
        {
            logger.Warn(e, "Caught exception thrown by trayIcon.UpdateIcon()");
        }

        try
        {
            trayIcon.UpdateToolTip(toolTip);
        }
        catch (Exception e)
        {
            logger.Warn(e, "Caught exception thrown by trayIcon.UpdateToolTip()");
        }
    }
}
