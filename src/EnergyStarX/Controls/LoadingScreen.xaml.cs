using DependencyPropertyGenerator;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EnergyStarX.Controls;

[DependencyProperty<bool>("IsLoading", DefaultValue = false)]
public sealed partial class LoadingScreen : UserControl
{
    public LoadingScreen()
    {
        this.InitializeComponent();
    }

    partial void OnIsLoadingChanged(bool newValue)
    {
        MainGrid.Visibility = newValue ? Visibility.Visible : Visibility.Collapsed;
        LoadingProgressRing.IsActive = newValue;
    }
}
