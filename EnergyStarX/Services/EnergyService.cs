using EnergyStarX.Core.Interop;
using EnergyStarX.Helpers;
using Microsoft.Windows.System.Power;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace EnergyStarX.Services;

public class EnergyService
{
    private CancellationTokenSource cts = new();

    private readonly object lockObject = new();

    public bool ThrottleWhenPluggedIn
    {
        get => LocalSettings.ThrottleWhenPluggedIn;
        set
        {
            lock (lockObject)
            {
                if (PowerManager.PowerSourceKind == PowerSourceKind.AC)
                {
                    bool throttleStatusChanged = value ? StartThrottling() : StopThrottling();
                    if (throttleStatusChanged)
                    {
                        StatusChanged?.Invoke(this, Status);
                    }
                }

                LocalSettings.ThrottleWhenPluggedIn = value;
            }
        }
    }

    public EnergyStatus Status => new(EnergyManager.IsRunning, PowerManager.PowerSourceKind);

    public event EventHandler<EnergyStatus>? StatusChanged;

    public record EnergyStatus(bool IsThrottling, PowerSourceKind PowerSourceKind);

    public void Initialize()
    {
        lock (lockObject)
        {
            HookManager.SubscribeToWindowEvents();
            ApplyBypassProcessList(LocalSettings.BypassProcessListString);

            PowerManager_PowerSourceKindChanged(null, new object());
            PowerManager.PowerSourceKindChanged += PowerManager_PowerSourceKindChanged;
        }
    }

    public void Terminate()
    {
        lock (lockObject)
        {
            PowerManager.PowerSourceKindChanged -= PowerManager_PowerSourceKindChanged;
            StopThrottling();

            HookManager.UnsubscribeWindowEvents();
        };
    }

    public void ApplyAndSaveBypassProcessList(string bypassProcessListString)
    {
        lock (lockObject)
        {
            ApplyBypassProcessList(bypassProcessListString);
            LocalSettings.BypassProcessListString = bypassProcessListString;
            Logger.Info("BypassProcessList saved");
        }
    }

    public void ApplyBypassProcessList(string bypassProcessListString)
    {
        lock (lockObject)
        {
            bool wasRunning = EnergyManager.IsRunning;

            if (wasRunning)
            {
                StopThrottling();
            }

            HashSet<string> bypassProcessList = ParseBypassProcessList(bypassProcessListString);
#if DEBUG
            bypassProcessList.Add("devenv.exe");    // Visual Studio
#endif
            EnergyManager.BypassProcessList = bypassProcessList.ToImmutableHashSet();

            Logger.Info($"Update BypassProcessList:\n{string.Join(Environment.NewLine, bypassProcessList)}");

            if (wasRunning)
            {
                StartThrottling();
            }
        }
    }

    /// <summary>
    /// Returns true if "EnergyManager.IsRunning" changes after this method executes. Otherwise false.
    /// </summary>
    private bool StartThrottling()
    {
        lock (lockObject)
        {
            if (!EnergyManager.IsRunning)
            {
                Logger.Info("EnergyService starts throttling.");

                cts = new CancellationTokenSource();

                EnergyManager.ThrottleAllUserBackgroundProcesses();

                // Thread houseKeepingThread = new(new ThreadStart(HouseKeepingThreadProc));
                // houseKeepingThread.Start();

                _ = HouseKeeping(cts.Token);

                EnergyManager.IsRunning = true;

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Returns true if "EnergyManager.IsRunning" changes after this method executes. Otherwise false.
    /// </summary>
    private bool StopThrottling()
    {
        lock (lockObject)
        {
            if (EnergyManager.IsRunning)
            {
                Logger.Info("EnergyService stops throttling.");

                cts.Cancel();
                EnergyManager.RecoverAllUserProcesses();

                EnergyManager.IsRunning = false;

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private void PowerManager_PowerSourceKindChanged(object? sender, object e)
    {
        lock (lockObject)
        {
            if (PowerManager.PowerSourceKind == PowerSourceKind.DC) // Battery
            {
                Logger.Info("Power source changed to battery.");

                StartThrottling();
            }
            else if (PowerManager.PowerSourceKind == PowerSourceKind.AC)
            {
                Logger.Info("Power source changed to AC.");

                if (ThrottleWhenPluggedIn)
                {
                    StartThrottling();
                }
                else
                {
                    StopThrottling();
                }
            }

            StatusChanged?.Invoke(this, Status);
        }
    }

    private HashSet<string> ParseBypassProcessList(string bypassProcessListString)
    {
        HashSet<string> result = new();
        Regex doubleSlashRegex = new("//");

        using StringReader stringReader = new(bypassProcessListString);
        while (stringReader.ReadLine() is string line)
        {
            Match doubleSlashMatch = doubleSlashRegex.Match(line);
            string processName = (doubleSlashMatch.Success ? line[..doubleSlashMatch.Index] : line).Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(processName))
            {
                result.Add(processName);
            }
        }

        return result;
    }

    // private async void HouseKeepingThreadProc()
    // {
    //     Logger.Info("House keeping thread started.");
    //     while (!cts.IsCancellationRequested)
    //     {
    //         try
    //         {
    //             PeriodicTimer houseKeepingTimer = new(TimeSpan.FromMinutes(5));
    //             await houseKeepingTimer.WaitForNextTickAsync(cts.Token);
    //             EnergyManager.ThrottleAllUserBackgroundProcesses();
    //         }
    //         catch (OperationCanceledException)
    //         {
    //             break;
    //         }
    //     }
    //     Logger.Info("House keeping thread stopped.");
    // }

    private async Task HouseKeeping(CancellationToken cancellationToken)
    {
        Logger.Info("House keeping task started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                EnergyManager.ThrottleAllUserBackgroundProcesses();
                Logger.Info("House keeping task throttling background processes");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                Logger.Error("House keeping task error", e);
            }
        }

        Logger.Info("House keeping task stopped.");
    }
}
