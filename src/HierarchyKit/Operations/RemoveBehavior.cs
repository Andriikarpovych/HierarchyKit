namespace HierarchyKit.Operations;

/// <summary>
/// Defines how descendants are handled when a node is removed.
/// </summary>
public enum RemoveBehavior
{
    /// <summary>
    /// Reparents direct children to the removed node's parent, or makes them roots.
    /// </summary>
    PromoteChildren,

    /// <summary>
    /// Removes the node and all of its descendants.
    /// </summary>
    Cascade
}
