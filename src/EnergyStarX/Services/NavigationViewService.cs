using EnergyStarX.Helpers;
using EnergyStarX.Interfaces.Services;
using EnergyStarX.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics.CodeAnalysis;

namespace EnergyStarX.Services;

public class NavigationViewService : INavigationViewService
{
    private readonly INavigationService navigationService;

    private readonly IPageService pageService;

    private NavigationView? navigationView;

    public IList<object>? MenuItems => navigationView?.MenuItems;

    public object? SettingsItem => navigationView?.SettingsItem;

    public NavigationViewService(INavigationService navigationService, IPageService pageService)
    {
        this.navigationService = navigationService;
        this.pageService = pageService;
    }

    [MemberNotNull(nameof(navigationView))]
    public void Initialize(NavigationView navigationView)
    {
        this.navigationView = navigationView;
        this.navigationView.BackRequested += OnBackRequested;
        this.navigationView.ItemInvoked += OnItemInvoked;
    }

    public void UnregisterEvents()
    {
        if (navigationView != null)
        {
            navigationView.BackRequested -= OnBackRequested;
            navigationView.ItemInvoked -= OnItemInvoked;
        }
    }

    public NavigationViewItem? GetSelectedItem(Type pageType)
    {
        if (navigationView != null)
        {
            return GetSelectedItem(navigationView.MenuItems, pageType) ?? GetSelectedItem(navigationView.FooterMenuItems, pageType);
        }

        return null;
    }

    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) => navigationService.GoBack();

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            navigationService.NavigateTo(typeof(SettingsViewModel).FullName!, transitionInfo: args.RecommendedNavigationTransitionInfo);
        }
        else
        {
            NavigationViewItem? selectedItem = args.InvokedItemContainer as NavigationViewItem;

            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
            {
                navigationService.NavigateTo(pageKey, transitionInfo: args.RecommendedNavigationTransitionInfo);
            }
        }
    }

    private NavigationViewItem? GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
    {
        foreach (NavigationViewItem? item in menuItems.OfType<NavigationViewItem>())
        {
            if (IsMenuItemForPageType(item, pageType))
            {
                return item;
            }

            NavigationViewItem? selectedChild = GetSelectedItem(item.MenuItems, pageType);
            if (selectedChild != null)
            {
                return selectedChild;
            }
        }

        return null;
    }

    private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
    {
        if (menuItem.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
        {
            return pageService.GetPageType(pageKey) == sourcePageType;
        }

        return false;
    }
}
