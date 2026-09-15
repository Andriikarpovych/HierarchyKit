using HierarchyKit.Abstractions;

namespace HierarchyKit.Hierarchy;

/// <summary>
/// Represents the relations of a tree structure in a hierarchy.
/// </summary>
internal sealed class TreeRelations<TKey> : IHierarchyRelations<TKey>
    where TKey : notnull
{
    private Dictionary<TKey, TKey> _parents;

    private Dictionary<TKey, List<TKey>> _children;

    private readonly IEqualityComparer<TKey> _keyComparer;

    public TreeRelations(IEqualityComparer<TKey>? keyComparer = null)
    {
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;

        _parents = new Dictionary<TKey, TKey>(_keyComparer);

        _children = new Dictionary<TKey, List<TKey>>(_keyComparer);
    }

    public bool HasParent(TKey nodeId)
    {
        return _parents.ContainsKey(nodeId);
    }

    public bool TryGetParent(TKey nodeId, out TKey parentId)
    {
        return _parents.TryGetValue(nodeId, out parentId!);
    }

    public IReadOnlyCollection<TKey> GetChildren(TKey nodeId)
    {
        if (_children.TryGetValue(nodeId, out var children))
        {
            return children.ToArray();
        }

        return Array.Empty<TKey>();
    }

    public void Add(TKey parentId, TKey childId)
    {
        var parents = new Dictionary<TKey, TKey>(_parents, _keyComparer);
        var childrenByParent = CloneChildren();

        parents.Add(childId, parentId);

        if (!childrenByParent.TryGetValue(parentId, out var children))
        {
            children = new List<TKey>();

            childrenByParent.Add(parentId, children);
        }

        children.Add(childId);

        Commit(parents, childrenByParent);
    }

    public void Remove(TKey parentId, TKey childId)
    {
        if (!_parents.TryGetValue(childId, out var actualParentId) ||
            !_keyComparer.Equals(actualParentId, parentId))
        {
            return;
        }

        if (!_children.TryGetValue(actualParentId, out var currentChildren))
        {
            throw new InvalidOperationException(
                "The relation indexes are inconsistent: the parent does not contain the child list.");
        }

        var childIndex = FindChildIndex(currentChildren, childId);

        if (childIndex < 0)
        {
            throw new InvalidOperationException(
                "The relation indexes are inconsistent: the parent does not contain the child.");
        }

        var parents = new Dictionary<TKey, TKey>(_parents, _keyComparer);
        var childrenByParent = CloneChildren();

        parents.Remove(childId);

        var children = childrenByParent[actualParentId];
        children.RemoveAt(childIndex);

        if (children.Count == 0)
        {
            childrenByParent.Remove(actualParentId);
        }

        Commit(parents, childrenByParent);
    }

    // Moves a node to a new parent in the hierarchy.
    public void Move(TKey nodeId, TKey newParentId)
    {
        if (!_parents.TryGetValue(nodeId, out var currentParentId))
        {
            Add(newParentId, nodeId);
            return;
        }

        if (_keyComparer.Equals(currentParentId, newParentId))
        {
            return;
        }

        if (!_children.TryGetValue(currentParentId, out var currentChildren))
        {
            throw new InvalidOperationException(
                "The relation indexes are inconsistent: the parent does not contain the child list.");
        }

        var childIndex = FindChildIndex(currentChildren, nodeId);

        if (childIndex < 0)
        {
            throw new InvalidOperationException(
                "The relation indexes are inconsistent: the parent does not contain the child.");
        }

        var parents = new Dictionary<TKey, TKey>(_parents, _keyComparer);
        var childrenByParent = CloneChildren();
        var oldParentChildren = childrenByParent[currentParentId];
        var storedNodeId = oldParentChildren[childIndex];

        oldParentChildren.RemoveAt(childIndex);

        if (oldParentChildren.Count == 0)
        {
            childrenByParent.Remove(currentParentId);
        }

        if (!childrenByParent.TryGetValue(newParentId, out var newParentChildren))
        {
            newParentChildren = new List<TKey>();
            childrenByParent.Add(newParentId, newParentChildren);
        }

        newParentChildren.Add(storedNodeId);
        parents[nodeId] = newParentId;

        Commit(parents, childrenByParent);
    }

    public void Clear()
    {
        var parents = new Dictionary<TKey, TKey>(_keyComparer);
        var childrenByParent = new Dictionary<TKey, List<TKey>>(_keyComparer);

        Commit(parents, childrenByParent);
    }

    private Dictionary<TKey, List<TKey>> CloneChildren()
    {
        var childrenByParent = new Dictionary<TKey, List<TKey>>(
            _children.Count,
            _keyComparer);

        foreach (var pair in _children)
        {
            childrenByParent.Add(pair.Key, new List<TKey>(pair.Value));
        }

        return childrenByParent;
    }

    private int FindChildIndex(List<TKey> children, TKey childId)
    {
        for (var index = 0; index < children.Count; index++)
        {
            if (_keyComparer.Equals(children[index], childId))
            {
                return index;
            }
        }

        return -1;
    }

    private void Commit(Dictionary<TKey, TKey> parents, Dictionary<TKey, List<TKey>> childrenByParent)
    {
        _parents = parents;

        _children = childrenByParent;
    }
}
