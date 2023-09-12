using EnergyStarX.Contracts.Services;

namespace EnergyStarX.Test.Fakes;

public class FakeWindowService : IWindowService
{
    public bool WindowVisible { get; private set; } = false;

    public event EventHandler? WindowShowing;
    public event EventHandler? WindowHiding;
    public event EventHandler? AppExiting;

    public void Initialize()
    {
    }

    public void ShowAppWindow()
    {
        if (!WindowVisible)
        {
            WindowShowing?.Invoke(this, EventArgs.Empty);
            WindowVisible = true;
        }
    }

    public void HideAppWindow()
    {
        if (WindowVisible)
        {
            WindowHiding?.Invoke(this, EventArgs.Empty);
            WindowVisible = false;
        }
    }

    public void ExitApp()
    {
        AppExiting?.Invoke(this, EventArgs.Empty);
    }
}
