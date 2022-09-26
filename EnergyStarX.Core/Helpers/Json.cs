using Newtonsoft.Json;

namespace EnergyStarX.Core.Helpers;

public static class Json
{
    public static async Task<T> ToObject<T>(string value)
    {
        return await Task.Run<T>(() =>
        {
            return JsonConvert.DeserializeObject<T>(value);
        });
    }

    public static async Task<string> Stringify(object value)
    {
        return await Task.Run<string>(() =>
        {
            return JsonConvert.SerializeObject(value);
        });
    }
}
