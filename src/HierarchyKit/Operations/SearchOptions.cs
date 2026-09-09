namespace HierarchyKit.Search;

public sealed class SearchOptions
{
    public TraversalOptions Traversal { get; init; } = new();

    public bool IncludeStartingNode { get; init; }
}