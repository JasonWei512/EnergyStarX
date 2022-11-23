using EnergyStarX.Contracts.Services;
using EnergyStarX.Helpers;
using EnergyStarX.ViewModels;
using Microsoft.UI.Xaml;

namespace EnergyStarX.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        this.navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternal(LaunchActivatedEventArgs args)
    {
        navigationService.NavigateTo((Settings.FirstRun ? typeof(HelpViewModel) : typeof(HomeViewModel)).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}
