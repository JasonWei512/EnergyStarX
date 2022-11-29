using static EnergyStarX.Helpers.SettingsManager;

namespace EnergyStarX.Helpers;

public static class Settings
{
    public static bool FirstRun
    {
        get => GetSetting("FirstRun", true);
        set => SetSetting("FirstRun", value);
    }

    public static bool ThrottleWhenPluggedIn
    {
        get => GetSetting("ThrottleWhenPluggedIn", false);
        set => SetSetting("ThrottleWhenPluggedIn", value);
    }

    public static string ProcessWhitelistString
    {
        get => GetSetting("BypassProcessListString", "DefaultProcessWhitelist".GetLocalized());
        set => SetSetting("BypassProcessListString", value);
    }
}
