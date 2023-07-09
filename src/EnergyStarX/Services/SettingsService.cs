using EnergyStarX.Helpers;
using EnergyStarX.Interfaces.Services;
using static EnergyStarX.Helpers.SettingsHelper;

namespace EnergyStarX.Services;

public class SettingsService : ISettingsService
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

    public Version LastRunVersion
    {
        get
        {
            if (Version.TryParse(GetSetting("LastRunVersion", string.Empty), out Version? version))
            {
                if (version is not null)
                {
                    return version;
                }
            }
            return new Version();
        }
        set
        {
            SetSetting("LastRunVersion", value.ToString());
        }
    }
}
