// Source:
// https://github.com/microsoft/testfx/issues/366#issuecomment-1110639841

using System.Reflection;

namespace EnergyStarX.Test.Helpers;

public static class ObjectExtensions
{
    /// <summary>
    /// Invokes a private/public method on an object. Useful for unit testing.
    /// </summary>
    /// <typeparam name="T">Specifies the method invocation result type.</typeparam>
    /// <param name="obj">The object containing the method.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="parameters">Parameters to pass to the method.</param>
    /// <returns>The result of the method invocation.</returns>
    /// <exception cref="ArgumentException">When no such method exists on the object.</exception>
    /// <exception cref="ArgumentException">When the method invocation resulted in an object of different type, as the type param T.</exception>
    /// <example>
    /// class Test
    /// {
    ///   private string GetStr(string x, int y) => $"Success! {x} {y}";
    /// }
    ///
    /// var test = new Test();
    /// var res = test.Invoke&lt;string&gt;("GetStr", "testparam", 123);
    /// Console.WriteLine(res); // "Success! testparam 123"
    /// </example>
    public static T Invoke<T>(this object obj, string methodName, params object[] parameters)
    {
        MethodInfo? method = obj.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null)
        {
            throw new ArgumentException($"No private method \"{methodName}\" found in class \"{obj.GetType().Name}\"");
        }

        object? res = method.Invoke(obj, parameters);
        if (res is T)
        {
            return (T)res;
        }

        throw new ArgumentException($"Bad type parameter. Type parameter is of type \"{typeof(T).Name}\", whereas method invocation result is of type \"{res?.GetType().Name}\"");
    }
}