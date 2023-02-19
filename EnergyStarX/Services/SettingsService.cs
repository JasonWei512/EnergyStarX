using EnergyStarX.Helpers;
using static EnergyStarX.Helpers.SettingsManager;

namespace EnergyStarX.Services;

public class SettingsService
{
    public bool FirstRun
    {
        get => GetSetting("FirstRun", true);
        set => SetSetting("FirstRun", value);
    }

    public bool ThrottleWhenPluggedIn
    {
        get => GetSetting("ThrottleWhenPluggedIn", false);
        set => SetSetting("ThrottleWhenPluggedIn", value);
    }

    public string ProcessWhitelistString
    {
        get => GetSetting("BypassProcessListString", "DefaultProcessWhitelist".ToLocalized());
        set => SetSetting("BypassProcessListString", value);
    }

    public string ProcessBlacklistString
    {
        get => GetSetting("ProcessBlacklistString", "DefaultProcessBlacklist".ToLocalized());
        set => SetSetting("ProcessBlacklistString", value);
    }
}
