using Microsoft.Windows.ApplicationModel.DynamicDependency;

[assembly: WinUITestTarget(typeof(EnergyStarX.App))]

namespace EnergyStarX.Test;

[TestClass]
public class Initialize
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        // TODO: Initialize the appropriate version of the Windows App SDK.
        // This is required when testing MSIX apps that are framework-dependent on the Windows App SDK.
        // https://learn.microsoft.com/en-us/windows/apps/api-reference/cs-bootstrapper-apis/microsoft.windows.applicationmodel.dynamicdependency/microsoft.windows.applicationmodel.dynamicdependency.bootstrap#tryinitialize-methods
        Bootstrap.TryInitialize(0x00010003, out int _);
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        Bootstrap.Shutdown();
    }
}
