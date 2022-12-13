using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using Windows.ApplicationModel;
using Windows.Storage;

namespace EnergyStarX.Services;

/* Note:
MSIX's StartupTask does not let you run an app at startup *as admin*.
So I have to use a Windows schedule task for this. Snipaste (the Microsoft Store version) also takes this approach.
The downside is, currently MSIX's uninstalling process is not customizable, so I cannot delete this schedule task when uninstalling.
This means after uninstalling this app, the schedule task will still be in Windows Task Scheduler. Snipaste also has this problem.
Related discussion: https://github.com/microsoft/WindowsAppSDK/discussions/3061

Maybe I should use a MSIX packaged service: https://learn.microsoft.com/uwp/schemas/appxpackage/uapmanifestschema/element-desktop6-service
It will be uninstalled automatically when the app is uninstalled. It can also throttle system services in session 0.
But that requires a double-process and communication app model, which is more complex to implement. 
*/
public class StartupService
{
    public enum StartupType
    {
        None = 0,
        User = 1,
        Admin = 2
    }

    public async Task Initialize()
    {
        // When app is initializing, if both MSIX StartupTask and admin schedule task are enabled, disable the MSIX StartupTask.
        await GetStartupType();
    }

    public async Task<StartupType> GetStartupType()
    {
        bool msixStartupTaskEnabled = await GetMsixStartupTaskEnabled();
        bool adminScheduleTaskExists = GetAdminScheduleTaskExists();

        StartupType startupType = (msixStartupTaskEnabled, adminScheduleTaskExists) switch
        {
            (false, false) => StartupType.None,
            (true, false) => StartupType.User,
            (false, true) => StartupType.Admin,
            (true, true) => StartupType.Admin
        };

        if (msixStartupTaskEnabled && adminScheduleTaskExists)
        {
            await DisableMsixStartupTask();
        }

        return startupType;
    }

    /// <summary>
    /// Returns whether StartupType set successfully.
    /// </summary>
    public async Task<bool> SetStartupType(StartupType newStartupType)
    {
        StartupType previousStartupType = await GetStartupType();

        return (previousStartupType, newStartupType) switch
        {
            (StartupType.None, StartupType.User) => await EnableMsixStartupTask(),
            (StartupType.None, StartupType.Admin) => await CreateAdminScheduleTask(),

            (StartupType.User, StartupType.None) => await DisableMsixStartupTask(),
            (StartupType.User, StartupType.Admin) => await CreateAdminScheduleTask() && await DisableMsixStartupTask(),

            (StartupType.Admin, StartupType.None) => await DeleteAdminScheduleTask(),
            (StartupType.Admin, StartupType.User) => await DeleteAdminScheduleTask() && await EnableMsixStartupTask(),

            _ when previousStartupType == newStartupType => true,
            _ => throw new ArgumentException($"Unknown StartupType transition: {previousStartupType} -> {newStartupType}")
        };
    }

    #region MSIX StartupTask

    private async Task<bool> GetMsixStartupTaskEnabled()
    {
        StartupTask startupTask = await StartupTask.GetAsync(App.Guid);
        return startupTask.State == StartupTaskState.Enabled;
    }

    /// <summary>
    /// Returns whether MSIX StartupTask enabled successfully.
    /// </summary>
    private async Task<bool> EnableMsixStartupTask()
    {
        StartupTask startupTask = await StartupTask.GetAsync(App.Guid);
        return await startupTask.RequestEnableAsync() == StartupTaskState.Enabled;
    }

    /// <summary>
    /// Returns whether MSIX StartupTask disabled successfully.
    /// </summary>
    private async Task<bool> DisableMsixStartupTask()
    {
        StartupTask startupTask = await StartupTask.GetAsync(App.Guid);
        startupTask.Disable();
        return true;
    }

    #endregion

    #region Unmanaged Admin Schedule Task

    private const string AdminScheduleTaskName = "EnergyStarXAdminStartupTask";

    private bool GetAdminScheduleTaskExists()
    {
        return Microsoft.Win32.TaskScheduler.TaskService.Instance.RootFolder.Tasks.Exists(AdminScheduleTaskName);
    }

    /// <summary>
    /// Returns whether schedule task created successfully. 
    /// Requires UAC.
    /// </summary>
    private async Task<bool> CreateAdminScheduleTask()
    {
        string scheduleTaskXml = $"""
            <?xml version="1.0" encoding="UTF-16"?>
            <Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
            <RegistrationInfo>
                <Description>Launch Energy Star X as admin at startup.</Description>
                <URI>\{AdminScheduleTaskName}</URI>
            </RegistrationInfo>
            <Triggers>
                <LogonTrigger>
                    <Enabled>true</Enabled>
                    <UserId>{WindowsIdentity.GetCurrent().Name}</UserId>
                </LogonTrigger>
            </Triggers>
            <Principals>
                <Principal id="Author">
                    <LogonType>InteractiveToken</LogonType>
                    <RunLevel>HighestAvailable</RunLevel>
                </Principal>
            </Principals>
            <Settings>
                <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
                <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
                <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
                <AllowHardTerminate>true</AllowHardTerminate>
                <StartWhenAvailable>false</StartWhenAvailable>
                <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
                <IdleSettings>
                    <StopOnIdleEnd>false</StopOnIdleEnd>
                    <RestartOnIdle>false</RestartOnIdle>
                </IdleSettings>
                <AllowStartOnDemand>true</AllowStartOnDemand>
                <Enabled>true</Enabled>
                <Hidden>false</Hidden>
                <RunOnlyIfIdle>false</RunOnlyIfIdle>
                <WakeToRun>false</WakeToRun>
                <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
                <Priority>7</Priority>
            </Settings>
            <Actions Context="Author">
                <Exec>
                    <Command>{GetExecutablePath()}</Command>
                    <Arguments>--silent</Arguments>
                </Exec>
            </Actions>
            </Task>
            """;

        StorageFile scheduleTaskXmlFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"{Guid.NewGuid()}.xml", CreationCollisionOption.OpenIfExists);
        await FileIO.WriteTextAsync(scheduleTaskXmlFile, scheduleTaskXml);

        Process process = new()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "schtasks",
                ArgumentList = { "/create", "/tn", AdminScheduleTaskName, "/XML", scheduleTaskXmlFile.Path, "/f" },

                UseShellExecute = true,
                Verb = "runas", // Run as admin
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        try
        {
            process.Start();
            await process.WaitForExitAsync();
        }
        catch (Win32Exception)
        {
            // UAC cancelled by user
            return false;
        }
        finally
        {
            await scheduleTaskXmlFile.DeleteAsync();
        }

        return GetAdminScheduleTaskExists();
    }

    /// <summary>
    /// Returns whether schedule task created successfully. 
    /// Requires UAC.
    /// </summary>
    private async Task<bool> DeleteAdminScheduleTask()
    {
        Process process = new()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "schtasks",
                ArgumentList = { "/delete", "/tn", AdminScheduleTaskName, "/f" },

                UseShellExecute = true,
                Verb = "runas", // Run as admin
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        try
        {
            process.Start();
            await process.WaitForExitAsync();
        }
        catch (Win32Exception)
        {
            // UAC cancelled by user
            return false;
        }

        return !GetAdminScheduleTaskExists();
    }

    private string GetExecutablePath()
    {
        return Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft",
            "WindowsApps",
            "EnergyStarX.exe"
        );
    }

    #endregion

}