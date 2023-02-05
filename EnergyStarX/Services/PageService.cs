using CommunityToolkit.Mvvm.ComponentModel;

using EnergyStarX.Contracts.Services;
using EnergyStarX.ViewModels;
using EnergyStarX.Views;

using Microsoft.UI.Xaml.Controls;

namespace EnergyStarX.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> pages = new();

    public PageService()
    {
        Configure<HelpViewModel, HelpPage>();
        Configure<SettingsViewModel, SettingsPage>();
        Configure<LogViewModel, LogPage>();
        Configure<HomeViewModel, HomePage>();
        Configure<DonateViewModel, DonatePage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (pages)
        {
            if (!pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (pages)
        {
            string? key = typeof(VM).FullName!;
            if (pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            Type? type = typeof(V);
            if (pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {pages.First(p => p.Value == type).Key}");
            }

            pages.Add(key, type);
        }
    }
}
