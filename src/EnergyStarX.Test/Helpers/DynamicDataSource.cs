// Source:
// https://github.com/xunit/xunit/blob/a8819848732d7d42e2b36cc081305754a31b9547/src/xunit.v3.core/TheoryData.cs

using System.Collections;

namespace EnergyStarX.Test.Helpers;

/// <summary>
/// Provides data for DataTestMethod based on collection initialization syntax.
/// </summary>
public abstract class DynamicDataSource : IReadOnlyCollection<object?[]>
{
    private readonly List<object?[]> data = new();

    /// <inheritdoc/>
    public int Count => data.Count;

    /// <summary>
    /// Adds a row to the test data.
    /// </summary>
    /// <param name="values">The values to be added.</param>
    protected void AddRow(params object?[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        data.Add(values);
    }

    /// <summary>
    /// Adds multiple rows to the test data.
    /// </summary>
    /// <param name="rows">The rows to be added.</param>
    protected void AddRows(IEnumerable<object?[]> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        foreach (object?[] row in rows)
        {
            AddRow(row);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<object?[]> GetEnumerator() => data.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Represents a set of data for a DataTestMethod with a single parameter. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T">The parameter type.</typeparam>
public class DynamicDataSource<T> : DynamicDataSource
{
    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(IEnumerable<T> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRange(values.ToArray());
    }

    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(params T[] values)
    {
        AddRange(values);
    }

    /// <summary>
    /// Adds data to the test data set.
    /// </summary>
    /// <param name="p">The data value.</param>
    public void Add(T p) =>
        AddRow(p);

    /// <summary>
    /// Adds multiple data items to the test data set.
    /// </summary>
    /// <param name="values">The data values.</param>
    public void AddRange(params T[] values) =>
        AddRows(values.Select(x => new object?[] { x }));
}

/// <summary>
/// Represents a set of data for a DataTestMethod with 2 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
public class DynamicDataSource<T1, T2> : DynamicDataSource
{
    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(IEnumerable<(T1, T2)> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRange(values.ToArray());
    }

    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(params (T1, T2)[] values)
    {
        AddRange(values);
    }

    /// <summary>
    /// Adds data to the test data set.
    /// </summary>
    /// <param name="p1">The first data value.</param>
    /// <param name="p2">The second data value.</param>
    public void Add(T1 p1, T2 p2) =>
        AddRow(p1, p2);

    /// <summary>
    /// Adds multiple data items to the test data set.
    /// </summary>
    /// <param name="values">The data values.</param>
    public void AddRange(params (T1 p1, T2 p2)[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRows(values.Select(x => new object?[] { x.p1, x.p2 }));
    }
}

/// <summary>
/// Represents a set of data for a DataTestMethod with 3 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
public class DynamicDataSource<T1, T2, T3> : DynamicDataSource
{
    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(IEnumerable<(T1, T2, T3)> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRange(values.ToArray());
    }

    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(params (T1, T2, T3)[] values)
    {
        AddRange(values);
    }

    /// <summary>
    /// Adds data to the test data set.
    /// </summary>
    /// <param name="p1">The first data value.</param>
    /// <param name="p2">The second data value.</param>
    /// <param name="p3">The third data value.</param>
    public void Add(T1 p1, T2 p2, T3 p3) =>
        AddRow(p1, p2, p3);

    /// <summary>
    /// Adds multiple data items to the test data set.
    /// </summary>
    /// <param name="values">The data values.</param>
    public void AddRange(params (T1 p1, T2 p2, T3 p3)[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3 }));
    }
}

/// <summary>
/// Represents a set of data for a DataTestMethod with 4 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
public class DynamicDataSource<T1, T2, T3, T4> : DynamicDataSource
{
    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(IEnumerable<(T1, T2, T3, T4)> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRange(values.ToArray());
    }

    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(params (T1, T2, T3, T4)[] values)
    {
        AddRange(values);
    }

    /// <summary>
    /// Adds data to the test data set.
    /// </summary>
    /// <param name="p1">The first data value.</param>
    /// <param name="p2">The second data value.</param>
    /// <param name="p3">The third data value.</param>
    /// <param name="p4">The fourth data value.</param>
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4) =>
        AddRow(p1, p2, p3, p4);

    /// <summary>
    /// Adds multiple data items to the test data set.
    /// </summary>
    /// <param name="values">The data values.</param>
    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4)[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4 }));
    }
}

/// <summary>
/// Represents a set of data for a DataTestMethod with 5 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
public class DynamicDataSource<T1, T2, T3, T4, T5> : DynamicDataSource
{
    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(IEnumerable<(T1, T2, T3, T4, T5)> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRange(values.ToArray());
    }

    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(params (T1, T2, T3, T4, T5)[] values)
    {
        AddRange(values);
    }

    /// <summary>
    /// Adds data to the test data set.
    /// </summary>
    /// <param name="p1">The first data value.</param>
    /// <param name="p2">The second data value.</param>
    /// <param name="p3">The third data value.</param>
    /// <param name="p4">The fourth data value.</param>
    /// <param name="p5">The fifth data value.</param>
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) =>
        AddRow(p1, p2, p3, p4, p5);

    /// <summary>
    /// Adds multiple data items to the test data set.
    /// </summary>
    /// <param name="values">The data values.</param>
    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4, x.p5 }));
    }
}

/// <summary>
/// Represents a set of data for a DataTestMethod with 6 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
public class DynamicDataSource<T1, T2, T3, T4, T5, T6> : DynamicDataSource
{
    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5, T6}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(IEnumerable<(T1, T2, T3, T4, T5, T6)> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRange(values.ToArray());
    }

    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5, T6}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(params (T1, T2, T3, T4, T5, T6)[] values)
    {
        AddRange(values);
    }

    /// <summary>
    /// Adds data to the test data set.
    /// </summary>
    /// <param name="p1">The first data value.</param>
    /// <param name="p2">The second data value.</param>
    /// <param name="p3">The third data value.</param>
    /// <param name="p4">The fourth data value.</param>
    /// <param name="p5">The fifth data value.</param>
    /// <param name="p6">The sixth data value.</param>
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) =>
        AddRow(p1, p2, p3, p4, p5, p6);

    /// <summary>
    /// Adds multiple data items to the test data set.
    /// </summary>
    /// <param name="values">The data values.</param>
    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4, x.p5, x.p6 }));
    }
}

/// <summary>
/// Represents a set of data for a DataTestMethod with 7 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <typeparam name="T7">The seventh parameter type.</typeparam>
public class DynamicDataSource<T1, T2, T3, T4, T5, T6, T7> : DynamicDataSource
{
    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5, T6, T7}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(IEnumerable<(T1, T2, T3, T4, T5, T6, T7)> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRange(values.ToArray());
    }

    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5, T6, T7}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(params (T1, T2, T3, T4, T5, T6, T7)[] values)
    {
        AddRange(values);
    }

    /// <summary>
    /// Adds data to the test data set.
    /// </summary>
    /// <param name="p1">The first data value.</param>
    /// <param name="p2">The second data value.</param>
    /// <param name="p3">The third data value.</param>
    /// <param name="p4">The fourth data value.</param>
    /// <param name="p5">The fifth data value.</param>
    /// <param name="p6">The sixth data value.</param>
    /// <param name="p7">The seventh data value.</param>
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) =>
        AddRow(p1, p2, p3, p4, p5, p6, p7);

    /// <summary>
    /// Adds multiple data items to the test data set.
    /// </summary>
    /// <param name="values">The data values.</param>
    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4, x.p5, x.p6, x.p7 }));
    }
}

/// <summary>
/// Represents a set of data for a DataTestMethod with 8 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <typeparam name="T7">The seventh parameter type.</typeparam>
/// <typeparam name="T8">The eighth parameter type.</typeparam>
public class DynamicDataSource<T1, T2, T3, T4, T5, T6, T7, T8> : DynamicDataSource
{
    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8)> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRange(values.ToArray());
    }

    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(params (T1, T2, T3, T4, T5, T6, T7, T8)[] values)
    {
        AddRange(values);
    }

    /// <summary>
    /// Adds data to the test data set.
    /// </summary>
    /// <param name="p1">The first data value.</param>
    /// <param name="p2">The second data value.</param>
    /// <param name="p3">The third data value.</param>
    /// <param name="p4">The fourth data value.</param>
    /// <param name="p5">The fifth data value.</param>
    /// <param name="p6">The sixth data value.</param>
    /// <param name="p7">The seventh data value.</param>
    /// <param name="p8">The eighth data value.</param>
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) =>
        AddRow(p1, p2, p3, p4, p5, p6, p7, p8);

    /// <summary>
    /// Adds multiple data items to the test data set.
    /// </summary>
    /// <param name="values">The data values.</param>
    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4, x.p5, x.p6, x.p7, x.p8 }));
    }
}

/// <summary>
/// Represents a set of data for a DataTestMethod with 9 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <typeparam name="T7">The seventh parameter type.</typeparam>
/// <typeparam name="T8">The eighth parameter type.</typeparam>
/// <typeparam name="T9">The ninth parameter type.</typeparam>
public class DynamicDataSource<T1, T2, T3, T4, T5, T6, T7, T8, T9> : DynamicDataSource
{
    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRange(values.ToArray());
    }

    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(params (T1, T2, T3, T4, T5, T6, T7, T8, T9)[] values)
    {
        AddRange(values);
    }

    /// <summary>
    /// Adds data to the test data set.
    /// </summary>
    /// <param name="p1">The first data value.</param>
    /// <param name="p2">The second data value.</param>
    /// <param name="p3">The third data value.</param>
    /// <param name="p4">The fourth data value.</param>
    /// <param name="p5">The fifth data value.</param>
    /// <param name="p6">The sixth data value.</param>
    /// <param name="p7">The seventh data value.</param>
    /// <param name="p8">The eighth data value.</param>
    /// <param name="p9">The ninth data value.</param>
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9) =>
        AddRow(p1, p2, p3, p4, p5, p6, p7, p8, p9);

    /// <summary>
    /// Adds multiple data items to the test data set.
    /// </summary>
    /// <param name="values">The data values.</param>
    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4, x.p5, x.p6, x.p7, x.p8, x.p9 }));
    }
}

/// <summary>
/// Represents a set of data for a DataTestMethod with 10 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <typeparam name="T7">The seventh parameter type.</typeparam>
/// <typeparam name="T8">The eighth parameter type.</typeparam>
/// <typeparam name="T9">The ninth parameter type.</typeparam>
/// <typeparam name="T10">The tenth parameter type.</typeparam>
public class DynamicDataSource<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : DynamicDataSource
{
    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRange(values.ToArray());
    }

    /// <summary>
    /// Initializes a new isntance of the <see cref="DynamicDataSource{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/> class.
    /// </summary>
    /// <param name="values">The initial set of values</param>
    public DynamicDataSource(params (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)[] values)
    {
        AddRange(values);
    }

    /// <summary>
    /// Adds data to the test data set.
    /// </summary>
    /// <param name="p1">The first data value.</param>
    /// <param name="p2">The second data value.</param>
    /// <param name="p3">The third data value.</param>
    /// <param name="p4">The fourth data value.</param>
    /// <param name="p5">The fifth data value.</param>
    /// <param name="p6">The sixth data value.</param>
    /// <param name="p7">The seventh data value.</param>
    /// <param name="p8">The eighth data value.</param>
    /// <param name="p9">The ninth data value.</param>
    /// <param name="p10">The tenth data value.</param>
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10) =>
        AddRow(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);

    /// <summary>
    /// Adds multiple data items to the test data set.
    /// </summary>
    /// <param name="values">The data values.</param>
    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4, x.p5, x.p6, x.p7, x.p8, x.p9, x.p10 }));
    }
}