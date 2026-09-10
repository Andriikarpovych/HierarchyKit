using HierarchyKit.Abstractions;

namespace HierarchyKit.Hierarchy;

/// <summary>
/// Represents the relations of a tree structure in a hierarchy.
/// </summary>
internal sealed class TreeRelations<TKey> : IHierarchyRelations<TKey>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TKey> _parents;

    private readonly Dictionary<TKey, List<TKey>> _children;

    private readonly IEqualityComparer<TKey> _keyComparer;

    public TreeRelations(
        IEqualityComparer<TKey>? keyComparer = null)
    {
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;

        _parents = new Dictionary<TKey, TKey>(_keyComparer);

        _children = new Dictionary<TKey, List<TKey>>(_keyComparer);
    }

    public bool HasParent(TKey nodeId)
    {
        return _parents.ContainsKey(nodeId);
    }

    public bool TryGetParent(
        TKey nodeId,
        out TKey parentId)
    {
        return _parents.TryGetValue(
            nodeId,
            out parentId!);
    }

    public IReadOnlyCollection<TKey> GetChildren(
        TKey nodeId)
    {
        if (_children.TryGetValue(
                nodeId,
                out var children))
        {
            return children.ToArray();
        }

        return Array.Empty<TKey>();
    }

    public void Add(TKey parentId, TKey childId)
    {
        _parents.Add(childId, parentId);

        if (!_children.TryGetValue(parentId, out var children))
        {
            children = new List<TKey>();

            _children.Add(parentId, children);
        }

        children.Add(childId);
    }

    public void Remove(TKey parentId, TKey childId)
    {
        _parents.Remove(childId);

        if (_children.TryGetValue(parentId, out var children))
        {
            children.Remove(childId);

            if (children.Count == 0)
            {
                _children.Remove(parentId);
            }
        }
    }

    public void Move(TKey nodeId, TKey newParentId)
    {
        if (!_parents.TryGetValue(
                nodeId,
                out var currentParentId))
        {
            // Root node.
            Add(newParentId, nodeId);

            return;
        }

        if (_keyComparer.Equals(currentParentId, newParentId))
        {
            return;
        }

        if (!_children.TryGetValue(newParentId, out var newParentChildren))
        {
            newParentChildren = new List<TKey>();
        }

        // Prepare all required structures
        // BEFORE changing existing state.

        newParentChildren.Add(nodeId);

        // Now mutation starts.

        var oldParentChildren = _children[currentParentId];

        oldParentChildren.Remove(nodeId);

        if (oldParentChildren.Count == 0)
        {
            _children.Remove(currentParentId);
        }

        _parents[nodeId] = newParentId;

        if (!_children.ContainsKey(newParentId))
        {
            _children.Add(newParentId, newParentChildren);
        }
    }

    public void Clear()
    {
        _parents.Clear();
        _children.Clear();
    }
}
