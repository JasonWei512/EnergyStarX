using EnergyStarX.Helpers;
using EnergyStarX.Interfaces.Services;
using EnergyStarX.Interfaces.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics.CodeAnalysis;

namespace EnergyStarX.Services;

// For more information on navigation between pages see
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
public class NavigationService : INavigationService
{
    private readonly IPageService pageService;
    private object? lastParameterUsed;
    private Frame? frame;

    public event NavigatedEventHandler? Navigated;

    public Frame? Frame
    {
        get
        {
            if (frame == null)
            {
                frame = App.MainWindow.Content as Frame;
                RegisterFrameEvents();
            }

            return frame;
        }

        set
        {
            UnregisterFrameEvents();
            frame = value;
            RegisterFrameEvents();
        }
    }

    [MemberNotNullWhen(true, nameof(Frame), nameof(frame))]
    public bool CanGoBack => Frame != null && Frame.CanGoBack;

    public NavigationService(IPageService pageService)
    {
        this.pageService = pageService;
    }

    private void RegisterFrameEvents()
    {
        if (frame != null)
        {
            frame.Navigated += OnNavigated;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (frame != null)
        {
            frame.Navigated -= OnNavigated;
        }
    }

    public bool GoBack()
    {
        if (CanGoBack)
        {
            object? vmBeforeNavigation = frame.GetPageViewModel();
            frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }

            return true;
        }

        return false;
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false, NavigationTransitionInfo? transitionInfo = null)
    {
        Type? pageType = pageService.GetPageType(pageKey);

        if (frame != null && (frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(lastParameterUsed))))
        {
            frame.Tag = clearNavigation;
            object? vmBeforeNavigation = frame.GetPageViewModel();

            bool navigated = transitionInfo is null ?
                frame.Navigate(pageType, parameter) :
                frame.Navigate(pageType, parameter, transitionInfo);

            if (navigated)
            {
                lastParameterUsed = parameter;
                if (vmBeforeNavigation is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
            }

            return navigated;
        }

        return false;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            bool clearNavigation = (bool)frame.Tag;
            if (clearNavigation)
            {
                frame.BackStack.Clear();
            }

            if (frame.GetPageViewModel() is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }

            Navigated?.Invoke(sender, e);
        }
    }
}
