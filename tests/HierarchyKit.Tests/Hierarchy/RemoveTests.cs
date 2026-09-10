using HierarchyKit.Exceptions;
using HierarchyKit.Operations;
using HierarchyKit.Tests.Models;

namespace HierarchyKit.Tests.Hierarchy;

public sealed class RemoveTests
{
    [Fact]
    public void Remove_ShouldPromoteChildrenToRemovedNodeParent_ByDefault()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var root = new TestNode(Guid.NewGuid());
        var node = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        hierarchy.Add(root);
        hierarchy.Add(node, root.Id);
        hierarchy.Add(child, node.Id);

        // Act
        hierarchy.Remove(node.Id);

        // Assert
        Assert.Equal(2, hierarchy.Count);
        Assert.False(hierarchy.Contains(node.Id));
        Assert.Throws<NodeNotFoundException>(() => hierarchy.Get(node.Id));
        Assert.Contains(hierarchy.GetChildren(root.Id), item => item.Id == child.Id);
    }

    [Fact]
    public void Remove_ShouldPromoteChildrenToRoots_WhenRemovingRoot()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var root = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());
        var grandChild = new TestNode(Guid.NewGuid());

        hierarchy.Add(root);
        hierarchy.Add(child, root.Id);
        hierarchy.Add(grandChild, child.Id);

        // Act
        hierarchy.Remove(root.Id);

        // Assert
        Assert.Equal(2, hierarchy.Count);
        Assert.False(hierarchy.Contains(root.Id));
        Assert.Contains(hierarchy.GetChildren(child.Id), item => item.Id == grandChild.Id);
    }

    [Fact]
    public void Remove_ShouldRemoveEntireSubtree_WhenBehaviorIsCascade()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var root = new TestNode(Guid.NewGuid());
        var node = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());
        var grandChild = new TestNode(Guid.NewGuid());
        var sibling = new TestNode(Guid.NewGuid());

        hierarchy.Add(root);
        hierarchy.Add(node, root.Id);
        hierarchy.Add(child, node.Id);
        hierarchy.Add(grandChild, child.Id);
        hierarchy.Add(sibling, root.Id);

        // Act
        hierarchy.Remove(node.Id, RemoveBehavior.Cascade);

        // Assert
        Assert.Equal(2, hierarchy.Count);
        Assert.False(hierarchy.Contains(node.Id));
        Assert.False(hierarchy.Contains(child.Id));
        Assert.False(hierarchy.Contains(grandChild.Id));
        Assert.True(hierarchy.Contains(root.Id));
        Assert.True(hierarchy.Contains(sibling.Id));
        Assert.DoesNotContain(hierarchy.GetChildren(root.Id), item => item.Id == node.Id);
        Assert.Contains(hierarchy.GetChildren(root.Id), item => item.Id == sibling.Id);
    }

    [Fact]
    public void Remove_ShouldThrow_WhenNodeDoesNotExist()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var node = new TestNode(Guid.NewGuid());

        // Act
        var action = () => hierarchy.Remove(node.Id);

        // Assert
        Assert.Throws<NodeNotFoundException>(action);
    }

    [Fact]
    public void Remove_ShouldRemoveRootAndAllDescendants_WhenBehaviorIsCascade()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var root = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        hierarchy.Add(root);
        hierarchy.Add(child, root.Id);

        // Act
        hierarchy.Remove(root.Id, RemoveBehavior.Cascade);

        // Assert
        Assert.Equal(0, hierarchy.Count);
        Assert.Empty(hierarchy.GetRoots());
    }

    [Fact]
    public void Remove_ShouldThrow_WhenBehaviorIsNotDefined()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var node = new TestNode(Guid.NewGuid());
        hierarchy.Add(node);

        // Act
        var action = () => hierarchy.Remove(node.Id, (RemoveBehavior)99);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
        Assert.True(hierarchy.Contains(node.Id));
    }
}
