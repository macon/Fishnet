// ReSharper disable CheckNamespace
namespace Fishnet.Core;

public interface ISome
{
}

/// <summary>
/// Generic wrapper for any value.
/// </summary>
/// <param name="value"></param>
/// <typeparam name="T"></typeparam>
public readonly struct Some<T>(T value) : ISome where T : notnull
{
    public T Value { get; } = value;
}
