namespace EnergyStarX.Contracts.Services;

public interface IDialogService
{
    Task<bool> ShowConfirmationDialog(string title, string? content = null);
}
