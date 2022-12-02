using Microsoft.Windows.ApplicationModel.Resources;

namespace EnergyStarX.Helpers;

public static class ResourceExtensions
{
    private static readonly ResourceLoader resourceLoader = new();

    public static string ToLocalized(this string resourceKey) => resourceLoader.GetString(resourceKey);
}
