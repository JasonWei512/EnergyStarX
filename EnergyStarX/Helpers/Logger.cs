using System.Diagnostics;

namespace EnergyStarX.Helpers;

public static class Logger
{
    public static void Log(object message)
    {
        if (message.ToString() is string value)
        {
            DateTime time = DateTime.Now;

            string prefix = $"{time} | ";
            string logString = $"{time} | {value.Replace("\n", $"\n{new string(' ', prefix.Length)}")}";

            Debug.WriteLine(logString);
            Console.WriteLine(logString);

            NewLogLine?.Invoke(null, new Message(time, value, logString));
        }
    }

    public static event EventHandler<Message>? NewLogLine;

    public record Message(DateTime Time, string Value, string LogString);
}
