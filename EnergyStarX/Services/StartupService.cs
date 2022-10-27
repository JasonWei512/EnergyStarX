using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using Windows.ApplicationModel;
using Windows.Storage;

namespace EnergyStarX.Services;

public class StartupService
{
    private const string AdminScheduleTaskName = "EnergyStarXStartupTask";

    public async Task Initialize()
    {
        bool msixStartupTaskEnabled = await GetMsixStartupTaskEnabled();
        bool adminScheduleTaskExists = GetAdminScheduleTaskExists();

        // TODO: Initialize the service
        throw new NotImplementedException();
    }

    private async Task<bool> GetMsixStartupTaskEnabled()
    {
        StartupTask startupTask = await StartupTask.GetAsync(App.Guid);
        return startupTask.State == StartupTaskState.Enabled;
    }

    private async Task EnableMsixStartupTask()
    {
        StartupTask startupTask = await StartupTask.GetAsync(App.Guid);
        await startupTask.RequestEnableAsync();
    }

    private async Task DisableMsixStartupTask()
    {
        StartupTask startupTask = await StartupTask.GetAsync(App.Guid);
        startupTask.Disable();
    }

    private bool GetAdminScheduleTaskExists()
    {
        return Microsoft.Win32.TaskScheduler.TaskService.Instance.RootFolder.Tasks.Exists(AdminScheduleTaskName);
    }

    /// <summary>
    /// Returns whether schedule task created successfully. 
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
                <Command>{GetExecutableName()}</Command>
                </Exec>
            </Actions>
            </Task>
            """;

        StorageFile scheduleTaskXmlFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"{Guid.NewGuid()}.xml", CreationCollisionOption.OpenIfExists);
        await FileIO.WriteTextAsync(scheduleTaskXmlFile, scheduleTaskXml);

        Process process = new()
        {
            StartInfo =
            {
                FileName = "schtasks",
                Arguments = $"/create /tn {AdminScheduleTaskName} /XML {scheduleTaskXmlFile.Path} /f",
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
            }
        };

        await scheduleTaskXmlFile.DeleteAsync();

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
        return true;
    }

    /// <summary>
    /// Returns whether schedule task created successfully. 
    /// </summary>
    private async Task<bool> DeleteAdminScheduleTask()
    {
        Process process = new()
        {
            StartInfo =
            {
                FileName = "schtasks",
                Arguments = $"/delete /tn {AdminScheduleTaskName} /f",
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
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
        return true;
    }

    private string GetExecutableName()
    {
        return Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft",
            "WindowsApps",
            "EnergyStarX.exe"
        );
    }
}