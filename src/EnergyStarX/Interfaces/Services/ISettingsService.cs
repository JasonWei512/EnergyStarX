namespace EnergyStarX.Interfaces.Services;

public interface ISettingsService
{
    bool FirstRun { get; set; }
    bool ThrottleWhenPluggedIn { get; set; }
    string ProcessWhitelistString { get; set; }
    string ProcessBlacklistString { get; set; }
    Version LastRunVersion { get; set; }
}