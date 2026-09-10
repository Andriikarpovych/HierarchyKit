namespace HierarchyKit.Abstractions;

/// <summary>
/// Defines the relationships between nodes in a hierarchy,
/// including parent-child relationships and operations to manage them.
/// </summary>
/// <typeparam name="TKey"></typeparam>
internal interface IHierarchyRelations<TKey> where TKey : notnull
{
    bool HasParent(TKey nodeId);

    bool TryGetParent(TKey nodeId, out TKey parentId);

    IReadOnlyCollection<TKey> GetChildren(TKey nodeId);

    void Add(TKey parentId, TKey childId);

    void Remove(TKey parentId, TKey childId);

    void Move(TKey nodeId, TKey newParentId);

    void Clear();
}
