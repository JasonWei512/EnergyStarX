using static EnergyStarX.Helpers.SettingsManager;

namespace EnergyStarX.Helpers;

public static class Settings
{
    public static bool FirstRun
    {
        get => GetSetting(nameof(FirstRun), true);
        set => SetSetting(nameof(FirstRun), value);
    }

    public static bool ThrottleWhenPluggedIn
    {
        get => GetSetting(nameof(ThrottleWhenPluggedIn), false);
        set => SetSetting(nameof(ThrottleWhenPluggedIn), value);
    }

    public static string BypassProcessListString
    {
        get => GetSetting(nameof(BypassProcessListString), "DefaultBypassProcessList".GetLocalized());
        set => SetSetting(nameof(BypassProcessListString), value);
    }
}
