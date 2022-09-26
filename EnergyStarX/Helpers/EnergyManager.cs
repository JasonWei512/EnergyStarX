// Copyright 2022 Bingxing Wang
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// If you are Microsoft (and/or its affiliates) employee, vendor or contractor who is working on Windows-specific integration projects, you may use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so without the restriction above.

using EnergyStarX.Core.Interop;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace EnergyStarX.Helpers;

internal unsafe class EnergyManager
{
    public static ImmutableHashSet<string> BypassProcessList { get; set; } = new HashSet<string>().ToImmutableHashSet();
    // Speical handling needs for UWP to get the child window process
    public const string UWPFrameHostApp = "ApplicationFrameHost.exe";

    private static uint pendingProcPid = 0;
    private static string pendingProcName = "";

    private static IntPtr pThrottleOn = IntPtr.Zero;
    private static IntPtr pThrottleOff = IntPtr.Zero;
    private static int szControlBlock = 0;

    public static bool IsRunning { get; set; }

    static EnergyManager()
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
    }

    private static void ToggleEfficiencyMode(IntPtr hProcess, bool enable)
    {
        Win32Api.SetProcessInformation(hProcess, Win32Api.PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
            enable ? pThrottleOn : pThrottleOff, (uint)szControlBlock);
        Win32Api.SetPriorityClass(hProcess, enable ? Win32Api.PriorityClass.IDLE_PRIORITY_CLASS : Win32Api.PriorityClass.NORMAL_PRIORITY_CLASS);
    }

    private static string GetProcessNameFromHandle(IntPtr hProcess)
    {
        int capacity = 1024;
        StringBuilder sb = new(capacity);

        if (Win32Api.QueryFullProcessImageName(hProcess, 0, sb, ref capacity))
        {
            return Path.GetFileName(sb.ToString());
        }

        return "";
    }

    public static unsafe void HandleForegroundEvent(IntPtr hwnd)
    {
        if (!IsRunning)
        {
            return;
        }

        uint windowThreadId = Win32Api.GetWindowThreadProcessId(hwnd, out uint procId);
        // This is invalid, likely a process is dead, or idk
        if (windowThreadId == 0 || procId == 0) return;

        IntPtr procHandle = Win32Api.OpenProcess(
            (uint)(Win32Api.ProcessAccessFlags.QueryLimitedInformation | Win32Api.ProcessAccessFlags.SetInformation), false, procId);
        if (procHandle == IntPtr.Zero) return;

        // Get the process
        string appName = GetProcessNameFromHandle(procHandle);

        // UWP needs to be handled in a special case
        if (appName == UWPFrameHostApp)
        {
            bool found = false;
            Win32Api.EnumChildWindows(hwnd, (innerHwnd, lparam) =>
            {
                if (found) return true;
                if (Win32Api.GetWindowThreadProcessId(innerHwnd, out uint innerProcId) > 0)
                {
                    if (procId == innerProcId) return true;

                    IntPtr innerProcHandle = Win32Api.OpenProcess((uint)(Win32Api.ProcessAccessFlags.QueryLimitedInformation |
                        Win32Api.ProcessAccessFlags.SetInformation), false, innerProcId);
                    if (innerProcHandle == IntPtr.Zero) return true;

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
        bool bypass = BypassProcessList.Contains(appName.ToLowerInvariant());
        if (!bypass)
        {
            Logger.Log($"Boosting {appName}");
            ToggleEfficiencyMode(procHandle, false);
        }

        if (pendingProcPid != 0)
        {
            Logger.Log($"Throttle {pendingProcName}");

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

    public static void ThrottleAllUserBackgroundProcesses()
    {
        Process[] runningProcesses = Process.GetProcesses();
        int currentSessionID = Process.GetCurrentProcess().SessionId;

        IEnumerable<Process> sameAsThisSession = runningProcesses.Where(p => p.SessionId == currentSessionID);
        foreach (Process proc in sameAsThisSession)
        {
            if (proc.Id == pendingProcPid) continue;
            if (BypassProcessList.Contains($"{proc.ProcessName}.exe".ToLowerInvariant())) continue;
            IntPtr hProcess = Win32Api.OpenProcess((uint)Win32Api.ProcessAccessFlags.SetInformation, false, (uint)proc.Id);
            ToggleEfficiencyMode(hProcess, true);
            Win32Api.CloseHandle(hProcess);
        }
    }

    public static void RecoverAllUserProcesses()
    {
        Process[] runningProcesses = Process.GetProcesses();
        int currentSessionID = Process.GetCurrentProcess().SessionId;

        IEnumerable<Process> sameAsThisSession = runningProcesses.Where(p => p.SessionId == currentSessionID);
        foreach (Process proc in sameAsThisSession)
        {
            if (BypassProcessList.Contains($"{proc.ProcessName}.exe".ToLowerInvariant())) continue;
            IntPtr hProcess = Win32Api.OpenProcess((uint)Win32Api.ProcessAccessFlags.SetInformation, false, (uint)proc.Id);
            ToggleEfficiencyMode(hProcess, false);
            Win32Api.CloseHandle(hProcess);
        }
    }
}
