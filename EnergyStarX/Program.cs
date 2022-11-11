using EnergyStarX.Helpers;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace EnergyStarX;

// Single instance mode:
// https://blogs.windows.com/windowsdeveloper/2022/01/28/making-the-app-single-instanced-part-3/
public static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        AppInstance mainInstance = AppInstance.FindOrRegisterForKey(App.Guid);
        if (!mainInstance.IsCurrent)
        {
            AppNotificationManager.Default.Show(new AppNotificationBuilder().AddText("AlreadyRunningMessage".GetLocalized()).BuildNotification());
            return;
        }

        WinRT.ComWrappersSupport.InitializeComWrappers();

        Application.Start(p =>
        {
            DispatcherQueueSynchronizationContext? context = new(DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);

            new App();
        });
    }
}
