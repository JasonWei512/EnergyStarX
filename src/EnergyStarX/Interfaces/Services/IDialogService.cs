namespace EnergyStarX.Interfaces.Services;

public interface IDialogService
{
    Task<bool> ShowConfirmationDialog(string title, string? content = null);
}
