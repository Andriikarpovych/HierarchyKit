using HierarchyKit.Abstractions;

namespace HierarchyKit.Tests.Models;

public sealed class StringTestNode : IHierarchyNode<string>
{
    public StringTestNode(string id)
    {
        Id = id;
    }

    public string Id { get; }
}
