using HierarchyKit.Exceptions;
using HierarchyKit.Tests.Models;

namespace HierarchyKit.Tests.Hierarchy;

public sealed class ComparerTests
{
    [Fact]
    public void Hierarchy_ShouldUseProvidedComparer_ForNodesAndRelations()
    {
        // Arrange
        var hierarchy = new Hierarchy<string, StringTestNode>(StringComparer.OrdinalIgnoreCase);
        var root = new StringTestNode("Root");
        var child = new StringTestNode("Child");

        hierarchy.Add(root);

        // Act
        hierarchy.Add(child, "ROOT");

        // Assert
        Assert.True(hierarchy.Contains("root"));
        Assert.Contains(hierarchy.GetChildren("root"), node => node.Id == child.Id);
        Assert.Throws<DuplicateNodeException>(() => hierarchy.Add(new StringTestNode("ROOT")));
    }
}
