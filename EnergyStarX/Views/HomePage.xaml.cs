using EnergyStarX.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace EnergyStarX.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
    }
}
