namespace HierarchyKit.Exceptions;

/// <summary>
/// Thrown when an operation would violate a hierarchy invariant.
/// </summary>
[Serializable]
public sealed class InvalidHierarchyOperationException : HierarchyException
{
    /// <summary>
    /// Initializes a new exception with the invariant-violation message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidHierarchyOperationException(string message) : base(message)
    {
    }
}
