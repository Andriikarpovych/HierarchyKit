namespace HierarchyKit.Search;

/// <summary>
/// Defines the order used to traverse descendants.
/// </summary>
public enum TraversalStrategy
{
    /// <summary>
    /// Visits a branch before continuing with its siblings.
    /// </summary>
    DepthFirst,

    /// <summary>
    /// Visits all nodes at one depth before continuing deeper.
    /// </summary>
    BreadthFirst
}
