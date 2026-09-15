using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using HierarchyKit;
using HierarchyKit.Abstractions;
using HierarchyKit.Operations;

BenchmarkRunner.Run<HierarchyBenchmarks>();

[MemoryDiagnoser]
public class HierarchyBenchmarks
{
    private const int BatchSize = 16;
    private const int RootId = 0;
    private const int SourceParentId = 1;
    private const int TargetParentId = 2;

    private Hierarchy<int, BenchmarkNode>[] _hierarchies = null!;

    [Params(100, 1_000, 5_000)]
    public int NodeCount { get; set; }

    [IterationSetup]
    public void Setup()
    {
        _hierarchies = new Hierarchy<int, BenchmarkNode>[BatchSize];

        for (var batchIndex = 0; batchIndex < BatchSize; batchIndex++)
        {
            var hierarchy = new Hierarchy<int, BenchmarkNode>();
            hierarchy.Add(new BenchmarkNode(RootId));
            hierarchy.Add(new BenchmarkNode(SourceParentId), RootId);
            hierarchy.Add(new BenchmarkNode(TargetParentId), RootId);

            for (var id = 3; id < NodeCount; id++)
            {
                hierarchy.Add(new BenchmarkNode(id), SourceParentId);
            }

            _hierarchies[batchIndex] = hierarchy;
        }
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public int Add()
    {
        var count = 0;

        for (var batchIndex = 0; batchIndex < BatchSize; batchIndex++)
        {
            var hierarchy = _hierarchies[batchIndex];
            hierarchy.Add(new BenchmarkNode(NodeCount + 1 + batchIndex), TargetParentId);
            count += hierarchy.Count;
        }

        return count;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public int Move()
    {
        var count = 0;

        for (var batchIndex = 0; batchIndex < BatchSize; batchIndex++)
        {
            var hierarchy = _hierarchies[batchIndex];
            hierarchy.Move(NodeCount - 1, TargetParentId);
            count += hierarchy.Count;
        }

        return count;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public int Remove()
    {
        var count = 0;

        for (var batchIndex = 0; batchIndex < BatchSize; batchIndex++)
        {
            var hierarchy = _hierarchies[batchIndex];
            hierarchy.Remove(NodeCount - 1, RemoveBehavior.Cascade);
            count += hierarchy.Count;
        }

        return count;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public int GetChildren()
    {
        var count = 0;

        for (var batchIndex = 0; batchIndex < BatchSize; batchIndex++)
        {
            count += _hierarchies[batchIndex].GetChildren(SourceParentId).Count;
        }

        return count;
    }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public int Descendants()
    {
        var count = 0;

        for (var batchIndex = 0; batchIndex < BatchSize; batchIndex++)
        {
            count += _hierarchies[batchIndex].Descendants(RootId).Count();
        }

        return count;
    }
}

public sealed record BenchmarkNode(int Id) : IHierarchyNode<int>;
