using EnergyStarX.Core.Interop;
using EnergyStarX.Helpers;
using Microsoft.Windows.System.Power;
using NLog;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace EnergyStarX.Services;

public class EnergyService
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private CancellationTokenSource cts = new();

    private readonly object lockObject = new();

    public bool ThrottleWhenPluggedIn
    {
        get => Settings.ThrottleWhenPluggedIn;
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

                Settings.ThrottleWhenPluggedIn = value;
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
            ApplyProcessWhitelist(Settings.ProcessWhitelistString);

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

    public void ApplyAndSaveProcessWhitelist(string processWhitelistString)
    {
        lock (lockObject)
        {
            ApplyProcessWhitelist(processWhitelistString);
            Settings.ProcessWhitelistString = processWhitelistString;
            logger.Info("ProcessWhitelist saved");
        }
    }

    public void ApplyProcessWhitelist(string processWhitelistString)
    {
        lock (lockObject)
        {
            bool wasRunning = EnergyManager.IsRunning;

            if (wasRunning)
            {
                StopThrottling();
            }

            HashSet<string> processWhitelist = ParseProcessWhitelist(processWhitelistString);
#if DEBUG
            processWhitelist.Add("devenv.exe");    // Visual Studio
#endif
            EnergyManager.ProcessWhitelist = processWhitelist.ToImmutableHashSet();

            logger.Info("Update ProcessWhitelist:\n{0}", string.Join(Environment.NewLine, processWhitelist));

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
                logger.Info("EnergyService starts throttling");
                cts = new CancellationTokenSource();
                EnergyManager.ThrottleAllUserBackgroundProcesses();
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
                logger.Info("EnergyService stops throttling");

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
                logger.Info("Power source changed to battery");

                StartThrottling();
            }
            else if (PowerManager.PowerSourceKind == PowerSourceKind.AC)
            {
                logger.Info("Power source changed to AC");

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

    private HashSet<string> ParseProcessWhitelist(string processWhitelistString)
    {
        HashSet<string> result = new();
        Regex doubleSlashRegex = new("//");

        using StringReader stringReader = new(processWhitelistString);
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

    private async Task HouseKeeping(CancellationToken cancellationToken)
    {
        logger.Info("House keeping task started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                lock (lockObject)
                {
                    EnergyManager.ThrottleAllUserBackgroundProcesses();
                }
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
}
