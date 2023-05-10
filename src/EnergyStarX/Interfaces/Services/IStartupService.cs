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

    Task<bool> SetStartupType(StartupType startupType);
}
