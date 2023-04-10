using Microsoft.UI.Xaml.Data;

namespace EnergyStarX.Converters;

public class OppositeBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => !(bool)value;

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
