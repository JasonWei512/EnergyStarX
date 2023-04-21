namespace EnergyStarX.Interfaces.Services;

public interface IWindowService
{
    bool WindowVisible { get; }

    event EventHandler? WindowShowing;
    event EventHandler? WindowHiding;
    event EventHandler? AppExiting;

    void Initialize();
    void ShowAppWindow();
    void HideAppWindow();
    void ExitApp();
}