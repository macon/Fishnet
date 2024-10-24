namespace Fishnet.Core;

/// <summary>
/// Simple ValueObject representing an error (not exception).
/// </summary>
/// <remarks>Hardcoded as E in Res&lt;T, E&gt; to create Res&lt;T&gt;.</remarks>
/// <param name="Message">Text describing the error.</param>
public record Error
{
    public string Message { get; }

    private Error(string message) => Message = message;

    public override string ToString() => Message;

    public static implicit operator Error(string message) => new(message);

    public static Error New(string message) =>
        message != null
            ? new Error(message)
            : throw new ArgumentNullException(nameof(message));
}
