namespace HierarchyKit.Exceptions;

/// <summary>
/// Thrown when an operation adds a node whose identifier already exists.
/// </summary>
[Serializable]
public sealed class DuplicateNodeException : HierarchyException
{
    /// <summary>
    /// Initializes a new exception for the duplicate identifier.
    /// </summary>
    /// <param name="id">The duplicate node identifier.</param>
    public DuplicateNodeException(object? id)
        : base($"A node with id '{id}' already exists in the hierarchy.")
    {
    }
}
