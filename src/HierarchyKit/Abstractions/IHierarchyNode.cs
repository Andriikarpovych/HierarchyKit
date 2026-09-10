namespace HierarchyKit.Abstractions;

/// <summary>
/// Represents a node in a hierarchical structure, identified by a unique key.
/// </summary>
/// <typeparam name="TKey">The type of the node identifier.</typeparam>
public interface IHierarchyNode<TKey> where TKey : notnull
{
    /// <summary>
    /// Gets the identifier that is unique within a hierarchy.
    /// </summary>
    TKey Id { get; }
}
