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

The current MVP targets .NET 10.

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

The domain model contains only its identity. `Hierarchy` owns all parent-child relationships.

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

## Status

The project is currently an MVP. NuGet publishing and CI will be added before the 1.0.0 release.

## Runnable example

The repository includes a commented console application that demonstrates the full lifecycle: adding nodes, querying roots and parents, DFS/BFS traversal, moving a node, and both removal behaviors.

Run it from the repository root:

```bash
dotnet run --project examples/HierarchyKit.Examples
```
