using HierarchyKit.Abstractions;

namespace HierarchyKit.Tests.Models;

public sealed class TestNode : IHierarchyNode<Guid>
{
    public TestNode(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}