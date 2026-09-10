namespace HierarchyKit.Exceptions;


/// <summary>
/// Represents the base exception for hierarchy operations.
/// </summary>
public class HierarchyException : Exception
{
    /// <summary>
    /// Initializes a new exception with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public HierarchyException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new exception with the specified message and underlying cause.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public HierarchyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
