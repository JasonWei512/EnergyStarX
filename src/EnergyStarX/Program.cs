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
    // Note that [STAThread] doesn't work with "async Task Main(string[] args)"
    // https://github.com/dotnet/roslyn/issues/22112
    [STAThread]
    private static void Main(string[] args)
    {
        AppInstance mainAppInstance = AppInstance.FindOrRegisterForKey(App.Guid);
        if (!mainAppInstance.IsCurrent)
        {
            AppNotification alreadyRunningNotification = new AppNotificationBuilder().AddText("AlreadyRunningMessage".ToLocalized()).BuildNotification();
            AppNotificationManager.Default.Show(alreadyRunningNotification);

            Task.Run(async () =>
            {
                // Clear the toast notification after several seconds
                await Task.Delay(TimeSpan.FromSeconds(4));
                await AppNotificationManager.Default.RemoveAllAsync();
            }).GetAwaiter().GetResult();

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
