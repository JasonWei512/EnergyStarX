using EnergyStarX.Contracts.Services;
using EnergyStarX.ViewModels;
using Microsoft.UI.Xaml;

namespace EnergyStarX.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService navigationService;
    private readonly ISettingsService settingsService;

    public DefaultActivationHandler(INavigationService navigationService, ISettingsService settingsService)
    {
        this.navigationService = navigationService;
        this.settingsService = settingsService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return navigationService.Frame?.Content == null;
    }

    protected override async Task HandleInternal(LaunchActivatedEventArgs args)
    {
        navigationService.NavigateTo((settingsService.FirstRun ? typeof(HelpViewModel) : typeof(HomeViewModel)).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}
