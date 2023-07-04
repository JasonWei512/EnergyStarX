namespace EnergyStarX.Interfaces.Services;

public enum StartupType
{
    None = 0,
    User = 1,
    Admin = 2
}

public interface IStartupService
{
    Task Initialize();

    Task<StartupType> GetStartupType();

    /// <summary>
    /// Returns whether StartupType set successfully.
    /// </summary>
    Task<bool> SetStartupType(StartupType startupType);
}
