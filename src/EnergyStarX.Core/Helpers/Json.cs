using System.Text.Json;

namespace EnergyStarX.Core.Helpers;

public static class Json
{
    public static async Task<T> ToObject<T>(string value) => await Task.Run(() => JsonSerializer.Deserialize<T>(value));

    public static async Task<string> Stringify(object value) => await Task.Run(() => JsonSerializer.Serialize(value));
}
