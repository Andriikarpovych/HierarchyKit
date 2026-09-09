namespace HierarchyKit.Abstractions;

public interface IHierarchyNode<TKey>
{
    TKey Id { get; }
}
