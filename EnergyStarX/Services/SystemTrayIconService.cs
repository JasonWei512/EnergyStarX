using CommunityToolkit.WinUI;
using EnergyStarX.Helpers;
using H.NotifyIcon.Core;
using Microsoft.UI.Dispatching;
using NLog;
using Windows.ApplicationModel;

namespace EnergyStarX.Services;

public class SystemTrayIconService
{
    private readonly System.Drawing.Icon ThrottlingIcon = new(Path.Combine(Package.Current.InstalledPath, "Assets/WindowIcon.ico"));
    private readonly string ThrottlingToolTip = "AppDisplayName".ToLocalized();

    private readonly System.Drawing.Icon NotThrottlingIcon = new(Path.Combine(Package.Current.InstalledPath, "Assets/WindowIcon-Gray.ico"));
    private readonly string NotThrottlingToolTip = $"{"AppDisplayName".ToLocalized()}\n({"Paused".ToLocalized()})";

    private readonly TrayIconWithContextMenu trayIcon = new();

    private readonly static Logger logger = LogManager.GetCurrentClassLogger();
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    private readonly WindowService windowsService;
    private readonly EnergyService energyService;

    public SystemTrayIconService(WindowService windowsService, EnergyService energyService)
    {
        this.windowsService = windowsService;
        this.energyService = energyService;
    }

    public async Task Initialize()
    {
        UpdateTrayIconImageAndToolTip(energyService.IsThrottling);

        trayIcon.ContextMenu = new PopupMenu()
        {
            Items =
            {
                new PopupMenuItem("Open".ToLocalized(), async (s, e) => await dispatcherQueue.EnqueueAsync(() => windowsService.ShowAppWindow())),
                new PopupMenuItem("Exit".ToLocalized(), async (s, e) => await dispatcherQueue.EnqueueAsync(() => windowsService.ExitApp()))
            }
        };

        trayIcon.MessageWindow.MouseEventReceived += async (s, e) =>
        {
            if (e.MouseEvent == MouseEvent.IconDoubleClick)
            {
                await dispatcherQueue.EnqueueAsync(() => windowsService.ShowAppWindow());
            }
        };

        trayIcon.Create();

        windowsService.AppExiting += WindowsService_AppExiting;
        energyService.StatusChanged += EnergyService_StatusChanged;

        // If user is using the taskbar enhancement tool StartAllBack, at this line the tray icon image may be wrong.
        // So I have to wait 0.1 second and update tray icon image and tooltip again.
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        UpdateTrayIconImageAndToolTip(energyService.IsThrottling);
    }

    private async void EnergyService_StatusChanged(object? sender, EnergyService.EnergyStatus e)
    {
        await dispatcherQueue.EnqueueAsync(() => UpdateTrayIconImageAndToolTip(e.IsThrottling));
    }

    private void WindowsService_AppExiting(object? sender, EventArgs e)
    {
        trayIcon.Remove();
        trayIcon.Dispose();

        ThrottlingIcon.Dispose();
        NotThrottlingIcon.Dispose();
    }

    private void UpdateTrayIconImageAndToolTip(bool isThrottling)
    {
        (System.Drawing.Icon icon, string toolTip) = GetTrayIconImageAndToolTip(isThrottling);

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

    private (System.Drawing.Icon Icon, string toolTip) GetTrayIconImageAndToolTip(bool isThrottling) =>
        isThrottling ?
        (ThrottlingIcon, ThrottlingToolTip) :
        (NotThrottlingIcon, NotThrottlingToolTip);
}
