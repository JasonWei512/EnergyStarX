using static EnergyStarX.Helpers.LocalSettingsHelper;

namespace EnergyStarX.Helpers;

public static class LocalSettings
{
    public static bool FirstRun
    {
        get => ReadPrimitive(nameof(FirstRun), true);
        set => SetPrimitive(nameof(FirstRun), value);
    }

    public static bool ThrottleWhenPluggedIn
    {
        get => ReadPrimitive(nameof(ThrottleWhenPluggedIn), false);
        set => SetPrimitive(nameof(ThrottleWhenPluggedIn), value);
    }

    public static string BypassProcessListString
    {
        get => ReadPrimitive(nameof(BypassProcessListString), "DefaultBypassProcessList".GetLocalized());
        set => SetPrimitive(nameof(BypassProcessListString), value);
    }
}
