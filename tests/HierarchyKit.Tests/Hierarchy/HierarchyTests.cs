using HierarchyKit.Exceptions;
using HierarchyKit.Tests.Models;

namespace HierarchyKit.Tests.Hierarchy;

public class HierarchyTests
{
    [Fact]
    public void Add_ShouldAddNode()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var node = new TestNode(Guid.NewGuid());

        // Act
        hierarchy.Add(node);

        // Assert
        Assert.Equal(1, hierarchy.Count);
        Assert.True(hierarchy.Contains(node.Id));
        Assert.Same(node, hierarchy.Get(node.Id));
    }

    [Fact]
    public void Add_ShouldThrow_WhenNodeWithSameIdAlreadyExists()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var id = Guid.NewGuid();

        hierarchy.Add(new TestNode(id));

        // Act & Assert
        Assert.Throws<DuplicateNodeException>(
            () => hierarchy.Add(new TestNode(id)));
    }

    [Fact]
    public void Get_ShouldThrow_WhenNodeDoesNotExist()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<NodeNotFoundException>(
            () => hierarchy.Get(id));
    }

    [Fact]
    public void TryGet_ShouldReturnFalse_WhenNodeDoesNotExist()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();

        // Act
        var result = hierarchy.TryGet(
            Guid.NewGuid(),
            out var node);

        // Assert
        Assert.False(result);
        Assert.Null(node);
    }

    [Fact]
    public void Add_ShouldAddChildToParent()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();

        var parent = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        hierarchy.Add(parent);

        // Act
        hierarchy.Add(child, parent.Id);

        // Assert
        Assert.Equal(2, hierarchy.Count);
    }

    [Fact]
    public void Add_ShouldThrow_WhenParentDoesNotExist()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();

        var parent = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<NodeNotFoundException>(() => hierarchy.Add(child, parent.Id));
        Assert.Equal(0, hierarchy.Count);
        Assert.False(hierarchy.Contains(child.Id));
    }

    [Fact]
    public void Add_ShouldThrow_WhenNodeIsItsOwnParent()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();

        var node = new TestNode(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<InvalidHierarchyOperationException>(() => hierarchy.Add(node, node.Id));
    }

    [Fact]
    public void GetRoots_ShouldReturnRootsInInsertionOrder()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var firstRoot = new TestNode(Guid.NewGuid());
        var secondRoot = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        hierarchy.Add(firstRoot);
        hierarchy.Add(secondRoot);
        hierarchy.Add(child, firstRoot.Id);

        // Act
        var roots = hierarchy.GetRoots();

        // Assert
        Assert.Equal(new[] { firstRoot, secondRoot }, roots);
    }

    [Fact]
    public void GetParent_ShouldReturnParentForChild()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var parent = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        hierarchy.Add(parent);
        hierarchy.Add(child, parent.Id);

        // Act
        var result = hierarchy.GetParent(child.Id);

        // Assert
        Assert.Same(parent, result);
    }

    [Fact]
    public void GetParent_ShouldThrow_WhenNodeIsRoot()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var root = new TestNode(Guid.NewGuid());

        hierarchy.Add(root);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => hierarchy.GetParent(root.Id));
    }

    [Fact]
    public void GetParent_ShouldThrow_WhenNodeDoesNotExist()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();

        // Act & Assert
        Assert.Throws<NodeNotFoundException>(
            () => hierarchy.GetParent(Guid.NewGuid()));
    }

    [Fact]
    public void TryGetParent_ShouldReturnParentForChild_AndFalseForRoot()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var root = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());

        hierarchy.Add(root);
        hierarchy.Add(child, root.Id);

        // Act
        var hasRootParent = hierarchy.TryGetParent(root.Id, out _);
        var hasChildParent = hierarchy.TryGetParent(child.Id, out var parent);

        // Assert
        Assert.False(hasRootParent);
        Assert.False(hierarchy.HasParent(root.Id));
        Assert.True(hasChildParent);
        Assert.True(hierarchy.HasParent(child.Id));
        Assert.Same(root, parent);
    }

    [Fact]
    public void Replace_ShouldPreserveNodeRelationships()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var root = new TestNode(Guid.NewGuid());
        var node = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());
        var replacement = new TestNode(node.Id);

        hierarchy.Add(root);
        hierarchy.Add(node, root.Id);
        hierarchy.Add(child, node.Id);

        // Act
        hierarchy.Replace(node.Id, replacement);

        // Assert
        Assert.Same(replacement, hierarchy.Get(node.Id));
        Assert.Contains(hierarchy.GetChildren(root.Id), item => ReferenceEquals(item, replacement));
        Assert.Contains(hierarchy.GetChildren(node.Id), item => item.Id == child.Id);
        Assert.True(hierarchy.TryGetParent(node.Id, out var parent));
        Assert.Same(root, parent);
    }

    [Fact]
    public void Replace_ShouldNotChangeHierarchy_WhenReplacementHasDifferentId()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var node = new TestNode(Guid.NewGuid());
        hierarchy.Add(node);

        // Act
        var action = () => hierarchy.Replace(node.Id, new TestNode(Guid.NewGuid()));

        // Assert
        Assert.Throws<InvalidHierarchyOperationException>(action);
        Assert.Same(node, hierarchy.Get(node.Id));
        Assert.Equal(1, hierarchy.Count);
    }

    [Fact]
    public void Clear_ShouldRemoveNodesAndRelationships()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var root = new TestNode(Guid.NewGuid());
        var child = new TestNode(Guid.NewGuid());
        hierarchy.Add(root);
        hierarchy.Add(child, root.Id);

        // Act
        hierarchy.Clear();
        hierarchy.Add(child);

        // Assert
        Assert.Equal(1, hierarchy.Count);
        Assert.Equal(new[] { child }, hierarchy.GetRoots());
        Assert.False(hierarchy.HasParent(child.Id));
    }
}
