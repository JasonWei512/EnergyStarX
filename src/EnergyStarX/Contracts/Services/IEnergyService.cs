namespace EnergyStarX.Contracts.Services;

public enum ThrottleStatus
{
    /// <summary>
    /// Possible situations: <br/>
    /// - Throttling paused by user <br/>
    /// - <see cref="EnergyService"/> is not initialized
    /// </summary>
    Stopped = 0,

    /// <summary>
    /// Device is plugged in, and <see cref="ISettingsService.ThrottleWhenPluggedIn"/> is disabled
    /// </summary>
    OnlyBlacklist = 1,

    /// <summary>
    /// Possible situations: <br/>
    /// - Device is on battery <br/>
    /// - Device is plugged in, and <see cref="ISettingsService.ThrottleWhenPluggedIn"/> is enabled
    /// </summary>
    BlacklistAndAllButWhitelist = 2
};

public interface IEnergyService
{
    ThrottleStatus ThrottleStatus { get; }

    bool PauseThrottling { get; set; }
    bool ThrottleWhenPluggedIn { get; set; }
    bool IsOnBattery { get; }

    /// <summary>
    /// Processes in whitelist will not be throttled
    /// </summary>
    IReadOnlySet<string> ProcessWhitelist { get; }

    /// <summary>
    /// Processes in blacklist will be throttled even when device is plugged in
    /// </summary>
    IReadOnlySet<string> ProcessBlacklist { get; }

    event EventHandler<ThrottleStatus>? ThrottleStatusChanged;

    void Initialize();

    void ApplyAndSaveProcessWhitelist(string processWhitelistString);
    void ApplyProcessWhitelist(string processWhitelistString);
    void ApplyAndSaveProcessBlacklist(string processBlacklistString);
    void ApplyProcessBlacklist(string processBlacklistString);
}
