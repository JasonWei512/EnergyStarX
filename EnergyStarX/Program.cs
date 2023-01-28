using EnergyStarX.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace EnergyStarX;

// Single instance mode:
// https://blogs.windows.com/windowsdeveloper/2022/01/28/making-the-app-single-instanced-part-3/
public static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        AppInstance mainAppInstance = AppInstance.FindOrRegisterForKey(App.Guid);
        if (!mainAppInstance.IsCurrent)
        {
            new ToastContentBuilder().AddText("AlreadyRunningMessage".ToLocalized()).Show();
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
