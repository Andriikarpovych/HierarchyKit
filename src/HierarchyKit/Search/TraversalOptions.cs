namespace HierarchyKit.Search;

public sealed class TraversalOptions
{
    public TraversalStrategy Strategy { get; init; } = TraversalStrategy.DepthFirst;

    public int? MinDepth { get; init; }

    public int? MaxDepth { get; init; }
}