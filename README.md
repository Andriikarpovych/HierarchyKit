# HierarchyKit

HierarchyKit is a .NET library for storing and operating on hierarchical data without adding hierarchy-specific state to domain objects. It supports a forest of trees: each node has zero or one parent, and any number of children.

## Features

- Stores parent-child relations separately from domain models.
- Prevents duplicate identifiers and cycles.
- Adds, moves, and removes nodes.
- Supports `PromoteChildren` and `Cascade` removal strategies.
- Traverses descendants with depth-first search by default or breadth-first search on demand.
- Traverses ancestors from the immediate parent to the root.
- Preserves node and sibling insertion order during traversal.

## Requirements

The current MVP targets .NET 8 and .NET 10 (`net8.0;net10.0`).

## Quick start

```csharp
using HierarchyKit;
using HierarchyKit.Abstractions;

public sealed record Department(Guid Id, string Name) : IHierarchyNode<Guid>;

var hierarchy = new Hierarchy<Guid, Department>();

var company = new Department(Guid.NewGuid(), "Company");
var engineering = new Department(Guid.NewGuid(), "Engineering");
var platform = new Department(Guid.NewGuid(), "Platform");

hierarchy.Add(company);
hierarchy.Add(engineering, company.Id);
hierarchy.Add(platform, engineering.Id);
```

The domain model only needs to expose its identity through `IHierarchyNode<TKey>` to participate in the hierarchy. `Hierarchy` owns all parent-child relationships.

## Querying the structure

```csharp
var roots = hierarchy.GetRoots();
var children = hierarchy.GetChildren(company.Id);

if (hierarchy.TryGetParent(platform.Id, out var parent))
{
    Console.WriteLine(parent.Name); // Engineering
}

var ancestors = hierarchy.Ancestors(platform.Id);
var descendants = hierarchy.Descendants(company.Id);
```

`GetChildren`, `GetRoots`, DFS, and BFS all preserve insertion order among sibling nodes. `Descendants` uses depth-first traversal by default.

Use breadth-first traversal or restrict results by depth with `SearchOptions`:

```csharp
using HierarchyKit.Search;

var breadthFirst = hierarchy.Descendants(
    company.Id,
    new SearchOptions
    {
        Traversal = new TraversalOptions
        {
            Strategy = TraversalStrategy.BreadthFirst,
            MaxDepth = 2
        }
    });
```

## Mutating the structure

Move a node and its complete subtree under a new parent:

```csharp
hierarchy.Move(platform.Id, company.Id);
```

Remove a node while promoting its direct children to its parent:

```csharp
hierarchy.Remove(engineering.Id);
```

Or remove a complete subtree:

```csharp
using HierarchyKit.Operations;

hierarchy.Remove(engineering.Id, RemoveBehavior.Cascade);
```

Replace the data object for an existing identifier without changing its parent or children:

```csharp
var renamedPlatform = new Department(platform.Id, "Core platform");
hierarchy.Replace(platform.Id, renamedPlatform);
```

Remove all nodes and relationships:

```csharp
hierarchy.Clear();
```

## Invariants

- Node identifiers are unique according to the configured `IEqualityComparer<TKey>`.
- A node cannot be its own parent.
- A node has at most one parent.
- Moving a node cannot create a cycle.
- Root nodes have no parent; a hierarchy may contain multiple roots.
- Every parent-child relation is represented consistently in both directions: a child listed under a parent has that parent, and every parent entry has the child in the corresponding child collection.
- A child cannot appear more than once in a parent's child collection.
- All relation lookups and mutations use the configured `IEqualityComparer<TKey>`.
- Sibling insertion order is preserved by relation mutations and queries.

## Mutation guarantees

Individual relationship mutations are designed to be failure-atomic: they either complete successfully or leave the relationship indexes unchanged. Relation state is prepared before it is committed, so an exception during a single `Add`, `Remove`, `Move`, or `Clear` relation mutation does not leave the parent and child indexes partially updated.

This is an exception-safety guarantee for a single relationship mutation, not a concurrency guarantee. It does not make the hierarchy thread-safe and does not provide a snapshot while another operation is running.

Composite operations such as `Remove` with `PromoteChildren` or `Cascade` perform multiple relationship mutations and are not a transaction covering the complete public operation. If a later step fails, earlier steps are not automatically rolled back.

## Thread safety

`Hierarchy` is not thread-safe.

Concurrent access and synchronization are the responsibility of the consumer. Callers must synchronize concurrent reads and mutations when a hierarchy instance is shared between threads.

Individual relationship mutations are designed to preserve internal relationship consistency and should either complete successfully or leave the relationship indexes unchanged. This guarantee applies to the relationship indexes for the individual mutation; it does not provide synchronization between concurrent operations or transactional rollback for composite hierarchy operations.

HierarchyKit does not synchronize mutations of the state of user-defined node objects. If node instances contain mutable business state, that state must be synchronized by the consumer independently of the hierarchy.

## Benchmarks

The repository contains a separate BenchmarkDotNet project for measuring the in-memory implementation:

```bash
dotnet run --project benchmarks/HierarchyKit.Benchmarks/HierarchyKit.Benchmarks.csproj -c Release -- --job short
```

The benchmark matrix covers hierarchies with 100, 1,000, and 5,000 nodes and measures `Add`, `Move`, `Remove`, `GetChildren`, and materialized `Descendants`. `MemoryDiagnoser` reports allocations in addition to elapsed time.

The mutation benchmarks are particularly useful for tracking the cost of the current failure-atomic copy-on-write implementation. Each mutation prepares copies of the relation indexes before commit, so its allocation and latency can grow with hierarchy size. Benchmark results are environment-dependent and should be compared on the same hardware and runtime configuration.

The benchmark fixture is intentionally simple: most nodes are siblings under one source parent, `Move` moves a leaf between two parents, and `Remove` removes a leaf with `Cascade`. These results describe that workload; deep trees, high branching factors, and subtree removal should be benchmarked separately before making capacity decisions.

Persistence and database access are outside the scope of these benchmarks and are not part of the current MVP.

## Publishing

The repository contains a GitHub Actions workflow for publishing to NuGet.org. To publish a
release:

1. Configure NuGet.org Trusted Publishing for this GitHub repository and workflow.
2. Add your NuGet.org username to the GitHub repository as an Actions secret named
   `NUGET_USER`.
3. Create and push a SemVer tag, for example `v0.1.0`:

   ```bash
   git tag v0.1.0
   git push origin v0.1.0
   ```

The workflow restores, tests both target frameworks, creates the package with the version from
the tag, and publishes the `.nupkg` and `.snupkg` files. NuGet package versions are immutable,
so each release must use a new version.

## Status

The project is currently an MVP.

## Runnable example

The repository includes a commented console application that demonstrates the full lifecycle: adding nodes, querying roots and parents, DFS/BFS traversal, moving a node, and both removal behaviors.

Run it from the repository root:

```bash
dotnet run --project examples/HierarchyKit.Examples
```
