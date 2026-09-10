namespace HierarchyKit.Search;

/// <summary>
/// Configures traversal order and depth boundaries.
/// </summary>
public sealed class TraversalOptions
{
    /// <summary>
    /// Gets the traversal order. The default is depth-first.
    /// </summary>
    public TraversalStrategy Strategy { get; init; } = TraversalStrategy.DepthFirst;

    /// <summary>
    /// Gets the inclusive minimum depth relative to the starting node.
    /// </summary>
    public int? MinDepth { get; init; }

    /// <summary>
    /// Gets the inclusive maximum depth relative to the starting node.
    /// </summary>
    public int? MaxDepth { get; init; }
}
