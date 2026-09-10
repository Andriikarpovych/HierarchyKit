using HierarchyKit.Abstractions;
using HierarchyKit.Exceptions;
using HierarchyKit.Hierarchy;
using HierarchyKit.Operations;
using HierarchyKit.Search;
using System.Diagnostics.CodeAnalysis;

namespace HierarchyKit;

/// <summary>
/// Stores nodes and their parent-child relationships in a forest of trees.
/// </summary>
/// <typeparam name="TKey">The non-nullable type used to identify nodes.</typeparam>
/// <typeparam name="TNode">The type of nodes stored in the hierarchy.</typeparam>
public sealed class Hierarchy<TKey, TNode>
    where TKey : notnull
    where TNode : IHierarchyNode<TKey>
{
    private readonly Dictionary<TKey, TNode> _nodes;

    private readonly List<TKey> _nodeOrder;

    private readonly IEqualityComparer<TKey> _keyComparer;

    private readonly IHierarchyRelations<TKey> _relations;

    /// <summary>
    /// Initializes a new hierarchy.
    /// </summary>
    /// <param name="keyComparer">The comparer used to identify node keys.</param>
    public Hierarchy(
        IEqualityComparer<TKey>? keyComparer = null)
    {
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;

        _nodes = new Dictionary<TKey, TNode>(_keyComparer);

        _nodeOrder = new List<TKey>();

        _relations = new TreeRelations<TKey>(_keyComparer);
    }

    /// <summary>
    /// Gets the number of nodes in the hierarchy.
    /// </summary>
    public int Count => _nodes.Count;

    /// <summary>
    /// Determines whether a node with the specified identifier exists.
    /// </summary>
    /// <param name="id">The node identifier.</param>
    /// <returns><see langword="true"/> when the node exists; otherwise, <see langword="false"/>.</returns>
    public bool Contains(TKey id)
    {
        return _nodes.ContainsKey(id);
    }

    /// <summary>
    /// Gets the node with the specified identifier.
    /// </summary>
    /// <param name="id">The node identifier.</param>
    /// <returns>The stored node.</returns>
    /// <exception cref="NodeNotFoundException">No node exists with <paramref name="id"/>.</exception>
    public TNode Get(TKey id)
    {
        if (_nodes.TryGetValue(id, out var node))
        {
            return node;
        }

        throw new NodeNotFoundException(id);
    }

    /// <summary>
    /// Attempts to get the node with the specified identifier.
    /// </summary>
    /// <param name="id">The node identifier.</param>
    /// <param name="node">The found node, or <see langword="null"/> when no matching node exists.</param>
    /// <returns><see langword="true"/> when the node exists; otherwise, <see langword="false"/>.</returns>
    public bool TryGet(TKey id, out TNode? node)
    {
        return _nodes.TryGetValue(id, out node);
    }

    /// <summary>
    /// Adds a node as a root of the hierarchy.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
    /// <exception cref="DuplicateNodeException">A node with the same identifier already exists.</exception>
    public void Add(TNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (!_nodes.TryAdd(node.Id, node))
        {
            throw new DuplicateNodeException(node.Id);
        }

        _nodeOrder.Add(node.Id);
    }

    /// <summary>
    /// Adds a node as a child of an existing node.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <param name="parentId">The identifier of the existing parent node.</param>
    /// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidHierarchyOperationException"><paramref name="node"/> is its own parent.</exception>
    /// <exception cref="NodeNotFoundException">The parent does not exist.</exception>
    /// <exception cref="DuplicateNodeException">A node with the same identifier already exists.</exception>
    public void Add(
        TNode node,
        TKey parentId)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (_keyComparer.Equals(node.Id, parentId))
        {
            throw new InvalidHierarchyOperationException(
                $"Node '{node.Id}' cannot be its own parent.");
        }

        if (!_nodes.ContainsKey(parentId))
        {
            throw new NodeNotFoundException(parentId);
        }

        if (!_nodes.TryAdd(node.Id, node))
        {
            throw new DuplicateNodeException(
                node.Id);
        }

        try
        {
            _relations.Add(parentId, node.Id);
            _nodeOrder.Add(node.Id);
        }
        catch
        {
            // Roll back node insertion.
            _nodes.Remove(node.Id);

            throw;
        }
    }

    /// <summary>
    /// Enumerates the direct children of a node.
    /// </summary>
    /// <param name="nodeId">The parent node identifier.</param>
    /// <returns>The direct child nodes, in insertion order. A leaf node produces an empty list.</returns>
    /// <exception cref="NodeNotFoundException">No node exists with <paramref name="nodeId"/>.</exception>
    public IReadOnlyList<TNode> GetChildren(TKey nodeId)
    {
        if (!_nodes.ContainsKey(nodeId))
        {
            throw new NodeNotFoundException(nodeId);
        }

        return _relations.GetChildren(nodeId).Select(Get).ToArray();
    }

    /// <summary>
    /// Gets root nodes in the order they were added to the hierarchy.
    /// </summary>
    /// <returns>The nodes that have no parent.</returns>
    public IReadOnlyList<TNode> GetRoots()
    {
        return _nodeOrder
            .Where(nodeId => !_relations.HasParent(nodeId))
            .Select(Get)
            .ToArray();
    }

    /// <summary>
    /// Determines whether a node has a parent.
    /// </summary>
    /// <param name="nodeId">The node identifier.</param>
    /// <returns><see langword="true"/> when the node has a parent; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NodeNotFoundException">No node exists with <paramref name="nodeId"/>.</exception>
    public bool HasParent(TKey nodeId)
    {
        _ = Get(nodeId);
        return _relations.HasParent(nodeId);
    }

    /// <summary>
    /// Attempts to get the parent of a node.
    /// </summary>
    /// <param name="nodeId">The node identifier.</param>
    /// <param name="parent">The parent node when this method returns <see langword="true"/>; otherwise, the default value.</param>
    /// <returns><see langword="true"/> when the node has a parent; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="NodeNotFoundException">No node exists with <paramref name="nodeId"/>.</exception>
    public bool TryGetParent(TKey nodeId, [MaybeNullWhen(false)] out TNode parent)
    {
        _ = Get(nodeId);

        if (!_relations.TryGetParent(nodeId, out var parentId))
        {
            parent = default!;
            return false;
        }

        parent = Get(parentId);
        return true;
    }

    /// <summary>
    /// Moves an existing node, together with its descendants, under a new parent.
    /// </summary>
    /// <param name="nodeId">The identifier of the node to move.</param>
    /// <param name="newParentId">The identifier of the new parent.</param>
    /// <exception cref="NodeNotFoundException">The node or new parent does not exist.</exception>
    /// <exception cref="InvalidHierarchyOperationException">The operation would make a node its own ancestor.</exception>
    public void Move(
        TKey nodeId,
        TKey newParentId)
    {
        // Validate that both the node and the new parent exist in the hierarchy
        if (!_nodes.ContainsKey(nodeId))
        {
            throw new NodeNotFoundException(nodeId);
        }

        // Validate that the new parent exists in the hierarchy
        if (!_nodes.ContainsKey(newParentId))
        {
            throw new NodeNotFoundException(newParentId);
        }

        // Validate that the node is not being moved under itself
        if (_keyComparer.Equals(nodeId, newParentId))
        {
            throw new InvalidHierarchyOperationException(
                $"Node '{nodeId}' cannot be moved under itself.");
        }

        //  Validate that moving the node under the new parent does not create a cycle
        if (WouldCreateCycle(nodeId, newParentId))
        {
            throw new InvalidHierarchyOperationException(
                $"Moving node '{nodeId}' under " +
                $"'{newParentId}' would create a cycle.");
        }

        _relations.Move(nodeId, newParentId);
    }

    /// <summary>
    /// Removes a node using the specified behavior for its descendants.
    /// </summary>
    /// <param name="nodeId">The identifier of the node to remove.</param>
    /// <param name="behavior">Determines whether descendants are promoted or removed with the node.</param>
    /// <exception cref="NodeNotFoundException">The node does not exist.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="behavior"/> is not a defined value.</exception>
    public void Remove(
        TKey nodeId,
        RemoveBehavior behavior = RemoveBehavior.PromoteChildren)
    {
        if (!_nodes.ContainsKey(nodeId))
        {
            throw new NodeNotFoundException(nodeId);
        }

        var hasParent = _relations.TryGetParent(nodeId, out var parentId);
        var childIds = _relations.GetChildren(nodeId);

        switch (behavior)
        {
            case RemoveBehavior.PromoteChildren:
                PromoteChildren(nodeId, hasParent, parentId, childIds);
                break;

            case RemoveBehavior.Cascade:
                RemoveSubtree(nodeId, hasParent, parentId, childIds);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(behavior), behavior, null);
        }
    }

    /// <summary>
    /// Replaces the stored node while preserving its position and relationships.
    /// </summary>
    /// <param name="nodeId">The identifier of the node to replace.</param>
    /// <param name="replacement">The replacement node, which must have the same identifier.</param>
    /// <exception cref="ArgumentNullException"><paramref name="replacement"/> is <see langword="null"/>.</exception>
    /// <exception cref="NodeNotFoundException">No node exists with <paramref name="nodeId"/>.</exception>
    /// <exception cref="InvalidHierarchyOperationException"><paramref name="replacement"/> has a different identifier.</exception>
    public void Replace(TKey nodeId, TNode replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);

        if (!_nodes.ContainsKey(nodeId))
        {
            throw new NodeNotFoundException(nodeId);
        }

        if (!_keyComparer.Equals(nodeId, replacement.Id))
        {
            throw new InvalidHierarchyOperationException(
                "A replacement node must have the same identifier as the node being replaced.");
        }

        _nodes[nodeId] = replacement;
    }

    /// <summary>
    /// Removes every node and relationship from the hierarchy.
    /// </summary>
    public void Clear()
    {
        _relations.Clear();
        _nodes.Clear();
        _nodeOrder.Clear();
    }

    /// <summary>
    /// Enumerates descendants using depth-first traversal.
    /// </summary>
    /// <param name="nodeId">The identifier of the node from which to start.</param>
    /// <returns>The descendants of <paramref name="nodeId"/>.</returns>
    public IEnumerable<TNode> Descendants(TKey nodeId)
    {
        return Descendants(nodeId, new SearchOptions());
    }

    /// <summary>
    /// Enumerates descendants using the specified search options.
    /// </summary>
    /// <param name="nodeId">The identifier of the node from which to start.</param>
    /// <param name="options">Traversal and depth options.</param>
    /// <returns>The matching nodes in traversal order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The depth range is invalid.</exception>
    /// <exception cref="NodeNotFoundException">The starting node does not exist.</exception>
    public IEnumerable<TNode> Descendants(TKey nodeId, SearchOptions options)
    {
        ValidateSearchOptions(options);
        _ = Get(nodeId);

        return EnumerateDescendants(nodeId, options);
    }

    /// <summary>
    /// Enumerates ancestors from the immediate parent to the root.
    /// </summary>
    /// <param name="nodeId">The identifier of the node from which to start.</param>
    /// <returns>The ancestors of <paramref name="nodeId"/>.</returns>
    public IEnumerable<TNode> Ancestors(TKey nodeId)
    {
        return Ancestors(nodeId, new SearchOptions());
    }

    /// <summary>
    /// Enumerates ancestors using the specified search options.
    /// </summary>
    /// <param name="nodeId">The identifier of the node from which to start.</param>
    /// <param name="options">Depth and starting-node options.</param>
    /// <returns>The matching nodes from nearest ancestor to root.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The depth range is invalid.</exception>
    /// <exception cref="NodeNotFoundException">The starting node does not exist.</exception>
    public IEnumerable<TNode> Ancestors(TKey nodeId, SearchOptions options)
    {
        ValidateSearchOptions(options);
        _ = Get(nodeId);

        return EnumerateAncestors(nodeId, options);
    }

    private bool WouldCreateCycle(TKey nodeId, TKey newParentId)
    {
        var currentId = newParentId;

        while (true)
        {
            if (_keyComparer.Equals(
                    currentId,
                    nodeId))
            {
                return true;
            }

            if (!_relations.TryGetParent(
                    currentId,
                    out var parentId))
            {
                return false;
            }

            currentId = parentId;
        }
    }

    private IEnumerable<TNode> EnumerateDescendants(TKey nodeId, SearchOptions options)
    {
        if (options.IncludeStartingNode && ShouldIncludeDepth(0, options.Traversal))
        {
            yield return Get(nodeId);
        }

        if (options.Traversal.MaxDepth == 0)
        {
            yield break;
        }

        if (options.Traversal.Strategy == TraversalStrategy.BreadthFirst)
        {
            var queue = new Queue<(TKey Id, int Depth)>();
            EnqueueChildren(nodeId, 1, queue);

            while (queue.Count > 0)
            {
                var (currentId, depth) = queue.Dequeue();

                if (ShouldIncludeDepth(depth, options.Traversal))
                {
                    yield return Get(currentId);
                }

                if (options.Traversal.MaxDepth is null || depth < options.Traversal.MaxDepth)
                {
                    EnqueueChildren(currentId, depth + 1, queue);
                }
            }

            yield break;
        }

        var stack = new Stack<(TKey Id, int Depth)>();

        PushChildren(nodeId, 1, stack);

        while (stack.Count > 0)
        {
            var (currentId, depth) = stack.Pop();

            if (ShouldIncludeDepth(depth, options.Traversal))
            {
                yield return Get(currentId);
            }

            if (options.Traversal.MaxDepth is null || depth < options.Traversal.MaxDepth)
            {
                PushChildren(currentId, depth + 1, stack);
            }
        }
    }

    private IEnumerable<TNode> EnumerateAncestors(TKey nodeId, SearchOptions options)
    {
        if (options.IncludeStartingNode && ShouldIncludeDepth(0, options.Traversal))
        {
            yield return Get(nodeId);
        }

        var depth = 1;
        var currentId = nodeId;

        while (_relations.TryGetParent(currentId, out var parentId))
        {
            if (options.Traversal.MaxDepth is not null && depth > options.Traversal.MaxDepth)
            {
                yield break;
            }

            if (ShouldIncludeDepth(depth, options.Traversal))
            {
                yield return Get(parentId);
            }

            currentId = parentId;
            depth++;
        }
    }

    private void EnqueueChildren(TKey parentId, int depth, Queue<(TKey Id, int Depth)> queue)
    {
        foreach (var childId in _relations.GetChildren(parentId))
        {
            queue.Enqueue((childId, depth));
        }
    }

    private void PushChildren(TKey parentId, int depth, Stack<(TKey Id, int Depth)> stack)
    {
        foreach (var childId in _relations.GetChildren(parentId).Reverse())
        {
            stack.Push((childId, depth));
        }
    }

    private static bool ShouldIncludeDepth(int depth, TraversalOptions options)
    {
        return (options.MinDepth is null || depth >= options.MinDepth)
            && (options.MaxDepth is null || depth <= options.MaxDepth);
    }

    private static void ValidateSearchOptions(SearchOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Traversal);

        if (options.Traversal.MinDepth is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "MinDepth cannot be negative.");
        }

        if (options.Traversal.MaxDepth is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "MaxDepth cannot be negative.");
        }

        if (options.Traversal.MinDepth > options.Traversal.MaxDepth)
        {
            throw new ArgumentException(
                "MinDepth cannot be greater than MaxDepth.",
                nameof(options));
        }

        if (!Enum.IsDefined(options.Traversal.Strategy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.Traversal.Strategy,
                "Unknown traversal strategy.");
        }
    }

    private void PromoteChildren(TKey nodeId, bool hasParent, TKey parentId, IReadOnlyCollection<TKey> childIds)
    {
        foreach (var childId in childIds)
        {
            if (hasParent)
            {
                _relations.Move(childId, parentId);
            }
            else
            {
                _relations.Remove(nodeId, childId);
            }
        }

        if (hasParent)
        {
            _relations.Remove(parentId, nodeId);
        }

        _nodes.Remove(nodeId);
        _nodeOrder.Remove(nodeId);
    }

    private void RemoveSubtree(TKey nodeId, bool hasParent, TKey parentId, IReadOnlyCollection<TKey> childIds)
    {
        var nodeIds = CollectSubtree(nodeId, childIds);

        if (hasParent)
        {
            _relations.Remove(parentId, nodeId);
        }

        for (var index = nodeIds.Count - 1; index > 0; index--)
        {
            var childId = nodeIds[index];

            if (_relations.TryGetParent(childId, out var childParentId))
            {
                _relations.Remove(childParentId, childId);
            }
        }

        foreach (var id in nodeIds)
        {
            _nodes.Remove(id);
            _nodeOrder.Remove(id);
        }
    }

    private List<TKey> CollectSubtree(TKey nodeId, IReadOnlyCollection<TKey> childIds)
    {
        var nodeIds = new List<TKey> { nodeId };
        var nodesToVisit = new Stack<TKey>();

        PushNodeIds(childIds, nodesToVisit);

        while (nodesToVisit.Count > 0)
        {
            var childId = nodesToVisit.Pop();
            nodeIds.Add(childId);
            PushNodeIds(_relations.GetChildren(childId), nodesToVisit);
        }

        return nodeIds;
    }

    private static void PushNodeIds(IReadOnlyCollection<TKey> nodeIds, Stack<TKey> target)
    {
        foreach (var nodeId in nodeIds.Reverse())
        {
            target.Push(nodeId);
        }
    }
}
