// Copyright 2022 Bingxing Wang
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// If you are Microsoft (and/or its affiliates) employee, vendor or contractor who is working on Windows-specific integration projects, you may use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so without the restriction above.

using EnergyStarX.Core.Interop;
using Microsoft.Windows.System.Power;
using NLog;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace EnergyStarX.Services;

public enum ThrottleStatus
{
    /// <summary>
    /// Possible situations: <br/>
    /// - Throttling paused by user <br/>
    /// - <see cref="EnergyService"/> is not initialized
    /// </summary>
    Stopped = 0,

    /// <summary>
    /// Device is plugged in, and <see cref="SettingsService.ThrottleWhenPluggedIn"/> is disabled
    /// </summary>
    OnlyBlacklist = 1,

    /// <summary>
    /// Possible situations: <br/>
    /// - Device is on battery <br/>
    /// - Device is plugged in, and <see cref="SettingsService.ThrottleWhenPluggedIn"/> is enabled
    /// </summary>
    BlacklistAndAllButWhitelist = 2
};

public class EnergyService
{
    private readonly static Logger logger = LogManager.GetCurrentClassLogger();
    private readonly object lockObject = new();
    private CancellationTokenSource houseKeepingCancellationTokenSource = new();

    private readonly WindowService windowService;
    private readonly SettingsService settingsService;

    // Speical handling needs for UWP to get the child window process
    private const string UWPFrameHostApp = "ApplicationFrameHost.exe";

    private readonly IntPtr pThrottleOn;
    private readonly IntPtr pThrottleOff;
    private readonly int szControlBlock;

    private uint pendingProcPid = 0;
    private string pendingProcName = "";

    public ThrottleStatus ThrottleStatus { get; private set; } = ThrottleStatus.Stopped;

    private bool pauseThrottling = false;

    public bool PauseThrottling
    {
        get => pauseThrottling;
        set
        {
            lock (lockObject)
            {
                if (pauseThrottling != value)
                {
                    pauseThrottling = value;
                    UpdateThrottleStatusAndNotify();
                }
            }
        }
    }

    public bool ThrottleWhenPluggedIn
    {
        get => settingsService.ThrottleWhenPluggedIn;
        set
        {
            lock (lockObject)
            {
                if (settingsService.ThrottleWhenPluggedIn != value)
                {
                    settingsService.ThrottleWhenPluggedIn = value;
                    UpdateThrottleStatusAndNotify();
                }
            }
        }
    }

    /// <summary>
    /// Processes in whitelist will not be throttled
    /// </summary>
    public IReadOnlySet<string> ProcessWhitelist { get; private set; } = new HashSet<string>();

    /// <summary>
    /// A subset of <see cref="ProcessWhitelist" />, where the process name contains "?" or "*".
    /// </summary>
    private IReadOnlySet<string> WildcardProcessWhitelist { get; set; } = new HashSet<string>();

    /// <summary>
    /// Processes in blacklist will be throttled even when device is plugged in
    /// </summary>
    public IReadOnlySet<string> ProcessBlacklist { get; private set; } = new HashSet<string>();

    /// <summary>
    /// A subset of <see cref="ProcessBlacklist" />, where the process name contains "?" or "*".
    /// </summary>
    private IReadOnlySet<string> WildcardProcessBlacklist { get; set; } = new HashSet<string>();

    public bool IsOnBattery => PowerManager.PowerSourceKind == PowerSourceKind.DC;

    public event EventHandler<ThrottleStatus>? ThrottleStatusChanged;

    public EnergyService(WindowService windowService, SettingsService settingsService)
    {
        szControlBlock = Marshal.SizeOf<Win32Api.PROCESS_POWER_THROTTLING_STATE>();
        pThrottleOn = Marshal.AllocHGlobal(szControlBlock);
        pThrottleOff = Marshal.AllocHGlobal(szControlBlock);

        Win32Api.PROCESS_POWER_THROTTLING_STATE throttleState = new()
        {
            Version = Win32Api.PROCESS_POWER_THROTTLING_STATE.PROCESS_POWER_THROTTLING_CURRENT_VERSION,
            ControlMask = Win32Api.ProcessorPowerThrottlingFlags.PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
            StateMask = Win32Api.ProcessorPowerThrottlingFlags.PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
        };

        Win32Api.PROCESS_POWER_THROTTLING_STATE unthrottleState = new()
        {
            Version = Win32Api.PROCESS_POWER_THROTTLING_STATE.PROCESS_POWER_THROTTLING_CURRENT_VERSION,
            ControlMask = Win32Api.ProcessorPowerThrottlingFlags.PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
            StateMask = Win32Api.ProcessorPowerThrottlingFlags.None,
        };

        Marshal.StructureToPtr(throttleState, pThrottleOn, false);
        Marshal.StructureToPtr(unthrottleState, pThrottleOff, false);

        this.windowService = windowService;
        this.settingsService = settingsService;

        this.windowService.AppExiting += WindowService_AppExiting;
    }

    public void Initialize()
    {
        lock (lockObject)
        {
            HookManager.SubscribeToWindowEvents();
            HookManager.SystemForegroundWindowChanged += HookManager_SystemForegroundWindowChanged;

            ApplyProcessWhitelist(settingsService.ProcessWhitelistString);
            ApplyProcessBlacklist(settingsService.ProcessBlacklistString);
            UpdateThrottleStatusAndNotify();
            PowerManager.PowerSourceKindChanged += PowerManager_PowerSourceKindChanged;
        }
    }

    private void WindowService_AppExiting(object? sender, EventArgs e)
    {
        lock (lockObject)
        {
            PowerManager.PowerSourceKindChanged -= PowerManager_PowerSourceKindChanged;
            StopThrottling(ThrottleStatus);

            HookManager.SystemForegroundWindowChanged -= HookManager_SystemForegroundWindowChanged;
            HookManager.UnsubscribeWindowEvents();
        };
    }

    public void ApplyAndSaveProcessWhitelist(string processWhitelistString)
    {
        lock (lockObject)
        {
            ApplyProcessWhitelist(processWhitelistString);
            settingsService.ProcessWhitelistString = processWhitelistString;
            logger.Info("ProcessWhitelist saved");
        }
    }

    public void ApplyProcessWhitelist(string processWhitelistString)
    {
        lock (lockObject)
        {
            ThrottleStatus previousThrottleStatus = ThrottleStatus;

            if (previousThrottleStatus != ThrottleStatus.Stopped)
            {
                StopThrottling(previousThrottleStatus);
            }

            (HashSet<string> processWhitelist, HashSet<string> wildcardProcessWhitelist) = ParseProcessList(processWhitelistString);
#if DEBUG
            processWhitelist.Add("devenv.exe");    // Visual Studio
#endif
            ProcessWhitelist = processWhitelist;
            WildcardProcessWhitelist = wildcardProcessWhitelist;

            logger.Info("Apply ProcessWhitelist:\n{0}", string.Join(Environment.NewLine, processWhitelist));

            if (previousThrottleStatus != ThrottleStatus.Stopped)
            {
                StartThrottling(previousThrottleStatus);
            }
        }
    }

    public void ApplyAndSaveProcessBlacklist(string processBlacklistString)
    {
        lock (lockObject)
        {
            ApplyProcessBlacklist(processBlacklistString);
            settingsService.ProcessBlacklistString = processBlacklistString;
            logger.Info("ProcessBlacklist saved");
        }
    }

    public void ApplyProcessBlacklist(string processBlacklistString)
    {
        lock (lockObject)
        {
            ThrottleStatus previousThrottleStatus = ThrottleStatus;

            if (previousThrottleStatus != ThrottleStatus.Stopped)
            {
                StopThrottling(previousThrottleStatus);
            }

            (HashSet<string> processBlacklist, HashSet<string> wildcardProcessBlacklist) = ParseProcessList(processBlacklistString);
            ProcessBlacklist = processBlacklist;
            WildcardProcessBlacklist = wildcardProcessBlacklist;

            logger.Info("Apply ProcessBlacklist:\n{0}", string.Join(Environment.NewLine, processBlacklist));

            if (previousThrottleStatus != ThrottleStatus.Stopped)
            {
                StartThrottling(previousThrottleStatus);
            }
        }
    }

    /// <summary>
    /// Get process name list from <paramref name="processListString"/>. 
    /// <br />
    /// Each line of <paramref name="processListString"/> contains one process name;
    /// Double slash and content after it in each line will be ignored.
    /// </summary>
    /// <returns>
    /// In the returned tuple, "wildcardProcessList" is a subset of "fullProcessList", where the process name contains "?" or "*".
    /// </returns>
    private (HashSet<string> fullProcessList, HashSet<string> wildcardProcessList) ParseProcessList(string processListString)
    {
        HashSet<string> fullProcessList = new();
        HashSet<string> wildcardProcessList = new();

        Regex doubleSlashRegex = new("//");

        using StringReader stringReader = new(processListString);
        while (stringReader.ReadLine() is string line)
        {
            Match doubleSlashMatch = doubleSlashRegex.Match(line);
            string processName = (doubleSlashMatch.Success ? line[..doubleSlashMatch.Index] : line).Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(processName))
            {
                fullProcessList.Add(processName);

                if (processName.Contains("?") || processName.Contains("*"))
                {
                    wildcardProcessList.Add(processName);
                }
            }
        }

        return (fullProcessList, wildcardProcessList);
    }

    /// <summary>
    /// Returns true if "ThrottleStatus" changes after this method executes. Otherwise false.
    /// </summary>
    private bool UpdateThrottleStatusAndNotify()
    {
        lock (lockObject)
        {
            ThrottleStatus fromThrottleStatus = ThrottleStatus;
            ThrottleStatus toThrottleStatus = (PauseThrottling, IsOnBattery, ThrottleWhenPluggedIn) switch
            {
                (true, _, _) => ThrottleStatus.Stopped,
                (false, true, _) => ThrottleStatus.BlacklistAndAllButWhitelist,
                (false, false, true) => ThrottleStatus.BlacklistAndAllButWhitelist,
                (false, false, false) => ThrottleStatus.OnlyBlacklist
            };

            bool throttleStatusChanged = (fromThrottleStatus, toThrottleStatus) switch
            {
                (ThrottleStatus.Stopped, ThrottleStatus.OnlyBlacklist) => StartThrottling(toThrottleStatus),
                (ThrottleStatus.Stopped, ThrottleStatus.BlacklistAndAllButWhitelist) => StartThrottling(toThrottleStatus),

                (ThrottleStatus.OnlyBlacklist, ThrottleStatus.Stopped) => StopThrottling(fromThrottleStatus),
                (ThrottleStatus.OnlyBlacklist, ThrottleStatus.BlacklistAndAllButWhitelist) => ThrottleUserBackgroundProcesses(toThrottleStatus),

                (ThrottleStatus.BlacklistAndAllButWhitelist, ThrottleStatus.Stopped) => StopThrottling(fromThrottleStatus),
                (ThrottleStatus.BlacklistAndAllButWhitelist, ThrottleStatus.OnlyBlacklist) => RecoverUserProcesses(fromThrottleStatus) && ThrottleUserBackgroundProcesses(toThrottleStatus),

                _ when fromThrottleStatus == toThrottleStatus => false,
                _ => throw new ArgumentException($"Unknown ThrottleStatus transition: {fromThrottleStatus} -> {toThrottleStatus}")
            };

            if (throttleStatusChanged)
            {
                ThrottleStatus = toThrottleStatus;
                ThrottleStatusChanged?.Invoke(this, ThrottleStatus);
                logger.Info("ThrottleStatus changed to: {0}", toThrottleStatus);
            }

            return throttleStatusChanged;
        }
    }

    /// <summary>
    /// Returns true if "ThrottleStatus" changes after this method executes. Otherwise false.
    /// </summary>
    private bool StartThrottling(ThrottleStatus toThrottleStatus)
    {
        lock (lockObject)
        {
            if (toThrottleStatus == ThrottleStatus.Stopped)
            {
                return false;
            }

            logger.Info("Start throttling");
            ThrottleUserBackgroundProcesses(toThrottleStatus);
            houseKeepingCancellationTokenSource = new CancellationTokenSource();
            _ = HouseKeeping(houseKeepingCancellationTokenSource.Token);

            return true;
        }
    }

    /// <summary>
    /// Returns true if "ThrottleStatus" changes after this method executes. Otherwise false.
    /// </summary>
    private bool StopThrottling(ThrottleStatus fromThrottleStatus)
    {
        lock (lockObject)
        {
            if (fromThrottleStatus == ThrottleStatus.Stopped)
            {
                return false;
            }

            logger.Info("Stop throttling");
            houseKeepingCancellationTokenSource.Cancel();
            RecoverUserProcesses(fromThrottleStatus);

            return true;
        }
    }

    private bool ThrottleUserBackgroundProcesses(ThrottleStatus toThrottleStatus)
    {
        lock (lockObject)
        {
            if (toThrottleStatus == ThrottleStatus.Stopped)
            {
                return false;
            }

            Process[] runningProcesses = Process.GetProcesses();
            int currentSessionID = Process.GetCurrentProcess().SessionId;

            IEnumerable<Process> sameAsThisSession = runningProcesses.Where(p => p.SessionId == currentSessionID);
            foreach (Process proc in sameAsThisSession)
            {
                if (proc.Id == pendingProcPid) { continue; }
                if (ShouldBypassProcess($"{proc.ProcessName}.exe".ToLowerInvariant(), toThrottleStatus)) { continue; }
                IntPtr hProcess = Win32Api.OpenProcess((uint)Win32Api.ProcessAccessFlags.SetInformation, false, (uint)proc.Id);
                ToggleEfficiencyMode(hProcess, true);
                Win32Api.CloseHandle(hProcess);
            }

            return true;
        }
    }

    private bool RecoverUserProcesses(ThrottleStatus fromThrottleStatus)
    {
        lock (lockObject)
        {
            if (fromThrottleStatus == ThrottleStatus.Stopped)
            {
                return false;
            }

            Process[] runningProcesses = Process.GetProcesses();
            int currentSessionID = Process.GetCurrentProcess().SessionId;

            IEnumerable<Process> sameAsThisSession = runningProcesses.Where(p => p.SessionId == currentSessionID);
            foreach (Process proc in sameAsThisSession)
            {
                if (ShouldBypassProcess($"{proc.ProcessName}.exe".ToLowerInvariant(), fromThrottleStatus)) { continue; }
                IntPtr hProcess = Win32Api.OpenProcess((uint)Win32Api.ProcessAccessFlags.SetInformation, false, (uint)proc.Id);
                ToggleEfficiencyMode(hProcess, false);
                Win32Api.CloseHandle(hProcess);
            }

            return true;
        }
    }

    private async Task HouseKeeping(CancellationToken cancellationToken)
    {
        logger.Info("House keeping task started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                ThrottleUserBackgroundProcesses(ThrottleStatus);
                logger.Info("House keeping task throttling background processes");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                logger.Error(e, "House keeping task error");
            }
        }

        logger.Info("House keeping task stopped.");
    }

    private void HookManager_SystemForegroundWindowChanged(object? sender, IntPtr hwnd)
    {
        lock (lockObject)
        {
            if (ThrottleStatus == ThrottleStatus.Stopped) { return; }

            uint windowThreadId = Win32Api.GetWindowThreadProcessId(hwnd, out uint procId);
            // This is invalid, likely a process is dead, or idk
            if (windowThreadId == 0 || procId == 0) { return; }

            IntPtr procHandle = Win32Api.OpenProcess(
                (uint)(Win32Api.ProcessAccessFlags.QueryLimitedInformation | Win32Api.ProcessAccessFlags.SetInformation), false, procId);
            if (procHandle == IntPtr.Zero) { return; }

            // Get the process
            string appName = GetProcessNameFromHandle(procHandle);

            // UWP needs to be handled in a special case
            if (appName == UWPFrameHostApp)
            {
                bool found = false;
                Win32Api.EnumChildWindows(hwnd, (innerHwnd, lparam) =>
                {
                    if (found) { return true; }
                    if (Win32Api.GetWindowThreadProcessId(innerHwnd, out uint innerProcId) > 0)
                    {
                        if (procId == innerProcId) { return true; }

                        IntPtr innerProcHandle = Win32Api.OpenProcess((uint)(Win32Api.ProcessAccessFlags.QueryLimitedInformation |
                            Win32Api.ProcessAccessFlags.SetInformation), false, innerProcId);
                        if (innerProcHandle == IntPtr.Zero) { return true; }

                        // Found. Set flag, reinitialize handles and call it a day
                        found = true;
                        Win32Api.CloseHandle(procHandle);
                        procHandle = innerProcHandle;
                        procId = innerProcId;
                        appName = GetProcessNameFromHandle(procHandle);
                    }

                    return true;
                }, IntPtr.Zero);
            }

            // Boost the current foreground app, and then impose EcoQoS for previous foreground app
            bool bypass = ShouldBypassProcess(appName, ThrottleStatus);
            if (!bypass)
            {
                logger.Info("Boosting {0}", appName);
                ToggleEfficiencyMode(procHandle, false);
            }

            if (pendingProcPid != 0)
            {
                logger.Info("Throttle {0}", pendingProcName);

                IntPtr prevProcHandle = Win32Api.OpenProcess((uint)Win32Api.ProcessAccessFlags.SetInformation, false, pendingProcPid);
                if (prevProcHandle != IntPtr.Zero)
                {
                    ToggleEfficiencyMode(prevProcHandle, true);
                    Win32Api.CloseHandle(prevProcHandle);
                    pendingProcPid = 0;
                    pendingProcName = "";
                }
            }

            if (!bypass)
            {
                pendingProcPid = procId;
                pendingProcName = appName;
            }

            Win32Api.CloseHandle(procHandle);
        }
    }

    private bool ShouldBypassProcess(string processName, ThrottleStatus throttleStatus) => !ShouldThrottleProcess(processName, throttleStatus);

    private bool ShouldThrottleProcess(string processName, ThrottleStatus throttleStatus)
    {
        return throttleStatus switch
        {
            ThrottleStatus.Stopped => false,
            ThrottleStatus.OnlyBlacklist => IsProcessInBlacklist(processName),
            ThrottleStatus.BlacklistAndAllButWhitelist => IsProcessInBlacklist(processName) || !IsProcessInWhitelist(processName),
            _ => throw new ArgumentException("Unknown ThrottleStatus")
        };
    }

    private bool IsProcessInWhitelist(string processName)
    {
        if (ProcessWhitelist.Contains(processName.ToLowerInvariant()))
        {
            return true;
        }

        if (WildcardProcessWhitelist.Any(wildcardExpression => FileSystemName.MatchesSimpleExpression(wildcardExpression, processName, true)))
        {
            return true;
        }

        return false;
    }

    private bool IsProcessInBlacklist(string processName)
    {
        if (ProcessBlacklist.Contains(processName.ToLowerInvariant()))
        {
            return true;
        }

        if (WildcardProcessBlacklist.Any(wildcardExpression => FileSystemName.MatchesSimpleExpression(wildcardExpression, processName, true)))
        {
            return true;
        }

        return false;
    }

    private void ToggleEfficiencyMode(IntPtr hProcess, bool enable)
    {
        Win32Api.SetProcessInformation(hProcess, Win32Api.PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
            enable ? pThrottleOn : pThrottleOff, (uint)szControlBlock);
        Win32Api.SetPriorityClass(hProcess, enable ? Win32Api.PriorityClass.IDLE_PRIORITY_CLASS : Win32Api.PriorityClass.NORMAL_PRIORITY_CLASS);
    }

    private void PowerManager_PowerSourceKindChanged(object? sender, object e)
    {
        lock (lockObject)
        {
            if (IsOnBattery)
            {
                logger.Info("Power source changed to battery");
            }
            else
            {
                logger.Info("Power source changed to AC");
            }

            UpdateThrottleStatusAndNotify();
        }
    }

    private string GetProcessNameFromHandle(IntPtr hProcess)
    {
        int capacity = 1024;
        StringBuilder sb = new(capacity);

        if (Win32Api.QueryFullProcessImageName(hProcess, 0, sb, ref capacity))
        {
            return Path.GetFileName(sb.ToString());
        }

        return "";
    }
}
