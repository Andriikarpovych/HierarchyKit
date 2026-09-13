using HierarchyKit.Search;
using HierarchyKit.Tests.Models;

namespace HierarchyKit.Tests.Hierarchy;

public sealed class TraversalTests
{
    [Fact]
    public void Descendants_ShouldUseDepthFirstTraversal_ByDefault()
    {
        // Arrange
        var hierarchy = CreateHierarchy(out var root, out var firstChild, out var secondChild, out var grandChild);

        // Act
        var result = hierarchy.Descendants(root.Id).Select(node => node.Id);

        // Assert
        Assert.Equal(new[] { firstChild.Id, grandChild.Id, secondChild.Id }, result);
    }

    [Fact]
    public void Descendants_ShouldAcceptStartingNode()
    {
        // Arrange
        var hierarchy = CreateHierarchy(out var root, out var firstChild, out var secondChild, out var grandChild);

        // Act
        var result = hierarchy.Descendants(root).Select(node => node.Id);

        // Assert
        Assert.Equal(new[] { firstChild.Id, grandChild.Id, secondChild.Id }, result);
    }

    [Fact]
    public void ForEach_ShouldExecuteActionForFilteredDescendants()
    {
        // Arrange
        var hierarchy = CreateHierarchy(out var root, out var firstChild, out var secondChild, out var grandChild);
        var visited = new List<Guid>();

        // Act
        hierarchy.Descendants(root)
            .Where(node => node.Id != secondChild.Id)
            .ForEach(node => visited.Add(node.Id));

        // Assert
        Assert.Equal(new[] { firstChild.Id, grandChild.Id }, visited);
    }


    [Fact]
    public void Descendants_ShouldUseBreadthFirstTraversal_WhenRequested()
    {
        // Arrange
        var hierarchy = CreateHierarchy(out var root, out var firstChild, out var secondChild, out var grandChild);
        var options = new SearchOptions
        {
            Traversal = new TraversalOptions
            {
                Strategy = TraversalStrategy.BreadthFirst
            }
        };

        // Act
        var result = hierarchy.Descendants(root.Id, options).Select(node => node.Id);

        // Assert
        Assert.Equal(new[] { firstChild.Id, secondChild.Id, grandChild.Id }, result);
    }

    [Fact]
    public void Descendants_ShouldRespectDepthAndStartingNodeOptions()
    {
        // Arrange
        var hierarchy = CreateHierarchy(out var root, out _, out _, out var grandChild);
        var options = new SearchOptions
        {
            IncludeStartingNode = true,
            Traversal = new TraversalOptions
            {
                MinDepth = 0,
                MaxDepth = 2
            }
        };

        // Act
        var result = hierarchy.Descendants(root.Id, options).Select(node => node.Id);

        // Assert
        Assert.Contains(root.Id, result);
        Assert.Contains(grandChild.Id, result);
        Assert.Equal(4, result.Count());
    }

    [Fact]
    public void Ancestors_ShouldReturnNearestAncestorFirst()
    {
        // Arrange
        var hierarchy = CreateHierarchy(out var root, out var firstChild, out _, out var grandChild);

        // Act
        var result = hierarchy.Ancestors(grandChild.Id).Select(node => node.Id);

        // Assert
        Assert.Equal(new[] { firstChild.Id, root.Id }, result);
    }

    [Fact]
    public void Descendants_ShouldThrow_WhenDepthRangeIsInvalid()
    {
        // Arrange
        var hierarchy = new Hierarchy<Guid, TestNode>();
        var root = new TestNode(Guid.NewGuid());
        hierarchy.Add(root);

        var options = new SearchOptions
        {
            Traversal = new TraversalOptions
            {
                MinDepth = 2,
                MaxDepth = 1
            }
        };

        // Act
        var action = () => hierarchy.Descendants(root.Id, options);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Ancestors_ShouldRespectDepthAndStartingNodeOptions()
    {
        // Arrange
        var hierarchy = CreateHierarchy(out var root, out var firstChild, out _, out var grandChild);
        var options = new SearchOptions
        {
            IncludeStartingNode = true,
            Traversal = new TraversalOptions
            {
                MinDepth = 0,
                MaxDepth = 1
            }
        };

        // Act
        var result = hierarchy.Ancestors(grandChild.Id, options).Select(node => node.Id);

        // Assert
        Assert.Equal(new[] { grandChild.Id, firstChild.Id }, result);
        Assert.DoesNotContain(root.Id, result);
    }

    [Fact]
    public void Descendants_ShouldReturnEmpty_WhenMinimumDepthExceedsTreeDepth()
    {
        // Arrange
        var hierarchy = CreateHierarchy(out var root, out _, out _, out _);
        var options = new SearchOptions
        {
            Traversal = new TraversalOptions
            {
                MinDepth = 3
            }
        };

        // Act
        var result = hierarchy.Descendants(root.Id, options);

        // Assert
        Assert.Empty(result);
    }

    private static Hierarchy<Guid, TestNode> CreateHierarchy(
        out TestNode root,
        out TestNode firstChild,
        out TestNode secondChild,
        out TestNode grandChild)
    {
        root = new TestNode(Guid.NewGuid());
        firstChild = new TestNode(Guid.NewGuid());
        secondChild = new TestNode(Guid.NewGuid());
        grandChild = new TestNode(Guid.NewGuid());

        var hierarchy = new Hierarchy<Guid, TestNode>();
        hierarchy.Add(root);
        hierarchy.Add(firstChild, root.Id);
        hierarchy.Add(secondChild, root.Id);
        hierarchy.Add(grandChild, firstChild.Id);

        return hierarchy;
    }
}
