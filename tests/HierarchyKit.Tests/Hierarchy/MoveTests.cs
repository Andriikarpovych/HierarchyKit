using HierarchyKit.Abstractions;
using HierarchyKit.Exceptions;

namespace HierarchyKit.Tests.Hierarchy;

public sealed class MoveTests
{
    [Fact]
    public void Move_ShouldMoveNodeToNewParent()
    {
        // Arrange
        var root = new TestNode(Guid.NewGuid());
        var parent1 = new TestNode(Guid.NewGuid());
        var parent2 = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        var hierarchy =
            new HierarchyKit.Hierarchy<Guid, TestNode>();

        hierarchy.Add(root);
        hierarchy.Add(parent1, root.Id);
        hierarchy.Add(parent2, root.Id);
        hierarchy.Add(child, parent1.Id);

        // Act
        hierarchy.Move(
            child.Id,
            parent2.Id);

        // Assert
        Assert.Contains(
            hierarchy.GetChildren(parent2.Id),
            node => node.Id == child.Id);

        Assert.DoesNotContain(
            hierarchy.GetChildren(parent1.Id),
            node => node.Id == child.Id);
    }

    [Fact]
    public void Move_ShouldRemoveNodeFromOldParent()
    {
        // Arrange
        var root = new TestNode(Guid.NewGuid());
        var oldParent = new TestNode(Guid.NewGuid());
        var newParent = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        var hierarchy =
            new HierarchyKit.Hierarchy<Guid, TestNode>();

        hierarchy.Add(root);
        hierarchy.Add(oldParent, root.Id);
        hierarchy.Add(newParent, root.Id);
        hierarchy.Add(child, oldParent.Id);

        // Act
        hierarchy.Move(
            child.Id,
            newParent.Id);

        // Assert
        Assert.DoesNotContain(
            hierarchy.GetChildren(oldParent.Id),
            node => node.Id == child.Id);
    }

    [Fact]
    public void Move_ShouldThrow_WhenNodeDoesNotExist()
    {
        // Arrange
        var root = new TestNode(Guid.NewGuid());
        var missingNodeId = Guid.NewGuid();

        var hierarchy =
            new HierarchyKit.Hierarchy<Guid, TestNode>();

        hierarchy.Add(root);

        // Act
        var action = () =>
            hierarchy.Move(
                missingNodeId,
                root.Id);

        // Assert
        Assert.Throws<NodeNotFoundException>(
            action);
    }

    [Fact]
    public void Move_ShouldThrow_WhenNewParentDoesNotExist()
    {
        // Arrange
        var root = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());
        var missingParentId = Guid.NewGuid();

        var hierarchy =
            new HierarchyKit.Hierarchy<Guid, TestNode>();

        hierarchy.Add(root);
        hierarchy.Add(child, root.Id);

        // Act
        var action = () =>
            hierarchy.Move(
                child.Id,
                missingParentId);

        // Assert
        Assert.Throws<NodeNotFoundException>(
            action);
    }

    [Fact]
    public void Move_ShouldThrow_WhenNodeIsMovedUnderItself()
    {
        // Arrange
        var root = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        var hierarchy =
            new HierarchyKit.Hierarchy<Guid, TestNode>();

        hierarchy.Add(root);
        hierarchy.Add(child, root.Id);

        // Act
        var action = () =>
            hierarchy.Move(
                child.Id,
                child.Id);

        // Assert
        Assert.Throws<InvalidHierarchyOperationException>(
            action);
    }

    [Fact]
    public void Move_ShouldThrow_WhenNewParentIsDescendant()
    {
        // Arrange
        var root = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());
        var grandChild = new TestNode(Guid.NewGuid());

        var hierarchy =
            new HierarchyKit.Hierarchy<Guid, TestNode>();

        hierarchy.Add(root);
        hierarchy.Add(child, root.Id);
        hierarchy.Add(grandChild, child.Id);

        // Act
        var action = () =>
            hierarchy.Move(
                root.Id,
                grandChild.Id);

        // Assert
        Assert.Throws<InvalidHierarchyOperationException>(
            action);
    }

    [Fact]
    public void Move_ShouldNotChangeHierarchy_WhenCycleDetected()
    {
        // Arrange
        var root = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());
        var grandChild = new TestNode(Guid.NewGuid());

        var hierarchy =
            new HierarchyKit.Hierarchy<Guid, TestNode>();

        hierarchy.Add(root);
        hierarchy.Add(child, root.Id);
        hierarchy.Add(grandChild, child.Id);

        // Act
        var action = () =>
            hierarchy.Move(
                root.Id,
                grandChild.Id);

        // Assert
        Assert.Throws<InvalidHierarchyOperationException>(
            action);

        // Original hierarchy must remain unchanged.

        Assert.Contains(
            hierarchy.GetChildren(root.Id),
            node => node.Id == child.Id);

        Assert.Contains(
            hierarchy.GetChildren(child.Id),
            node => node.Id == grandChild.Id);

        Assert.Empty(
            hierarchy.GetChildren(grandChild.Id));
    }

    [Fact]
    public void Move_ShouldAllowMovingRootUnderAnotherRoot()
    {
        // Arrange
        var root1 = new TestNode(Guid.NewGuid());
        var root2 = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        var hierarchy =
            new HierarchyKit.Hierarchy<Guid, TestNode>();

        hierarchy.Add(root1);
        hierarchy.Add(root2);
        hierarchy.Add(child, root1.Id);

        // Act
        hierarchy.Move(
            root1.Id,
            root2.Id);

        // Assert
        Assert.Contains(
            hierarchy.GetChildren(root2.Id),
            node => node.Id == root1.Id);

        Assert.Contains(
            hierarchy.GetChildren(root1.Id),
            node => node.Id == child.Id);
    }

    [Fact]
    public void Move_ShouldPreserveChildOrder_WhenMovingToNewParent()
    {
        // Arrange
        var root = new TestNode(Guid.NewGuid());
        var oldParent = new TestNode(Guid.NewGuid());
        var newParent = new TestNode(Guid.NewGuid());
        var existingChild = new TestNode(Guid.NewGuid());
        var movedChild = new TestNode(Guid.NewGuid());

        var hierarchy = new HierarchyKit.Hierarchy<Guid, TestNode>();
        hierarchy.Add(root);
        hierarchy.Add(oldParent, root.Id);
        hierarchy.Add(newParent, root.Id);
        hierarchy.Add(existingChild, newParent.Id);
        hierarchy.Add(movedChild, oldParent.Id);

        // Act
        hierarchy.Move(movedChild.Id, newParent.Id);

        // Assert
        Assert.Equal(
            new[] { existingChild.Id, movedChild.Id },
            hierarchy.GetChildren(newParent.Id).Select(node => node.Id));
    }

    [Fact]
    public void Move_ShouldNotChangeHierarchy_WhenNodeAlreadyHasNewParent()
    {
        // Arrange
        var root = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());
        var hierarchy = new HierarchyKit.Hierarchy<Guid, TestNode>();

        hierarchy.Add(root);
        hierarchy.Add(child, root.Id);

        // Act
        hierarchy.Move(child.Id, root.Id);

        // Assert
        Assert.Equal(new[] { child.Id }, hierarchy.GetChildren(root.Id).Select(node => node.Id));
        Assert.True(hierarchy.TryGetParent(child.Id, out var parent));
        Assert.Same(root, parent);
    }

    private sealed class TestNode
        : IHierarchyNode<Guid>
    {
        public TestNode(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}
