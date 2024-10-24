namespace Fishnet.Core;

public class FunctionalStateException : Exception
{
    public FunctionalStateException()
    {
    }

    public FunctionalStateException(string? message) : base(message)
    {
    }

    public FunctionalStateException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
