using EnergyStarX.Helpers;
using EnergyStarX.Interfaces.Services;
using NickJohn.WinUI.ObservableSettings;

namespace EnergyStarX.Services;

public partial class SettingsService : ISettingsService
{
    [ObservableSetting("FirstRun")]
    private readonly bool firstRun = true;

    [ObservableSetting("ThrottleWhenPluggedIn")]
    private readonly bool throttleWhenPluggedIn = false;

    [ObservableSetting("BypassProcessListString")]
    private readonly string processWhitelistString = "DefaultProcessWhitelist".ToLocalized();

    [ObservableSetting("ProcessBlacklistString")]
    private readonly string processBlacklistString = "DefaultProcessBlacklist".ToLocalized();
}
