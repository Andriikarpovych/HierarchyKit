namespace HierarchyKit.Search;

/// <summary>
/// Configures a hierarchy search or traversal operation.
/// </summary>
public sealed class SearchOptions
{
    /// <summary>
    /// Gets the traversal order and depth boundaries.
    /// </summary>
    public TraversalOptions Traversal { get; init; } = new();

    /// <summary>
    /// Gets whether the starting node is included at depth zero.
    /// </summary>
    public bool IncludeStartingNode { get; init; }
}
