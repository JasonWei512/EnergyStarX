using EnergyStarX.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace EnergyStarX.Views;

public sealed partial class DonatePage : Page
{
    public DonateViewModel ViewModel
    {
        get;
    }

    public DonatePage()
    {
        ViewModel = App.GetService<DonateViewModel>();
        InitializeComponent();
    }
}
