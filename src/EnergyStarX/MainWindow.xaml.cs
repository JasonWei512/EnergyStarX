using EnergyStarX.Helpers;
using Microsoft.UI.Xaml.Media;

namespace EnergyStarX;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();
        this.SystemBackdrop = new MicaBackdrop();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".ToLocalized();
    }
}
