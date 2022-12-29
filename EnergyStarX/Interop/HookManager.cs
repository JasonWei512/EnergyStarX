// Copyright 2022 Bingxing Wang
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// If you are Microsoft (and/or its affiliates) employee, vendor or contractor who is working on Windows-specific integration projects, you may use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so without the restriction above.

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace EnergyStarX.Core.Interop;

public static class HookManager
{
    private const int WINEVENT_INCONTEXT = 4;
    private const int WINEVENT_OUTOFCONTEXT = 0;
    private const int WINEVENT_SKIPOWNPROCESS = 2;
    private const int WINEVENT_SKIPOWNTHREAD = 1;

    private const int EVENT_SYSTEM_FOREGROUND = 3;

    private static IntPtr windowEventHook;
    // Explicitly declare it to prevent GC
    // See: https://stackoverflow.com/questions/6193711/call-has-been-made-on-garbage-collected-delegate-in-c
    private static WinEventProc hookProcDelegate = WindowEventCallback;

    /// <summary>
    /// The event arg is the handle to the window that is in the foreground.
    /// </summary>
    public static event EventHandler<IntPtr>? SystemForegroundWindowChanged;

    public static void SubscribeToWindowEvents()
    {
        if (windowEventHook == IntPtr.Zero)
        {
            windowEventHook = SetWinEventHook(
                EVENT_SYSTEM_FOREGROUND, // eventMin
                EVENT_SYSTEM_FOREGROUND, // eventMax
                IntPtr.Zero,             // hmodWinEventProc
                hookProcDelegate,        // lpfnWinEventProc
                0,                       // idProcess
                0,                       // idThread
                WINEVENT_OUTOFCONTEXT);

            if (windowEventHook == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    public static void UnsubscribeWindowEvents()
    {
        if (windowEventHook != IntPtr.Zero)
        {
            UnhookWinEvent(windowEventHook);
            windowEventHook = IntPtr.Zero;
        }
    }

    private static void WindowEventCallback(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        SystemForegroundWindowChanged?.Invoke(null, hwnd);
    }

    private delegate void WinEventProc(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWinEventHook(int eventMin, int eventMax,
        IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int UnhookWinEvent(IntPtr hWinEventHook);
}
