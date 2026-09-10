namespace HierarchyKit.Exceptions;

/// <summary>
/// Thrown when an operation references a node that does not exist.
/// </summary>
[Serializable]
public sealed class NodeNotFoundException : HierarchyException
{
    /// <summary>
    /// Initializes a new exception for the missing identifier.
    /// </summary>
    /// <param name="id">The missing node identifier.</param>
    public NodeNotFoundException(object? id)
        : base($"Node with id '{id}' was not found.")
    {
    }
}
