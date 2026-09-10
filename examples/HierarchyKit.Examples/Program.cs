using HierarchyKit;
using HierarchyKit.Abstractions;
using HierarchyKit.Operations;
using HierarchyKit.Search;

// A domain object needs only a stable identity. It does not store Parent or Children.
var hierarchy = new Hierarchy<Guid, Department>();

var company = new Department(Guid.NewGuid(), "Company");
var engineering = new Department(Guid.NewGuid(), "Engineering");
var operations = new Department(Guid.NewGuid(), "Operations");
var platform = new Department(Guid.NewGuid(), "Platform");
var qualityAssurance = new Department(Guid.NewGuid(), "Quality assurance");
var support = new Department(Guid.NewGuid(), "Support");

// Add a root, then use the parent's key when adding a child.
hierarchy.Add(company);
hierarchy.Add(engineering, company.Id);
hierarchy.Add(operations, company.Id);
hierarchy.Add(platform, engineering.Id);
hierarchy.Add(qualityAssurance, engineering.Id);
hierarchy.Add(support, operations.Id);

PrintTree(hierarchy, "Initial hierarchy");

// A hierarchy is a forest, so GetRoots can return more than one node.
Console.WriteLine($"Roots: {string.Join(", ", hierarchy.GetRoots().Select(node => node.Name))}");

// TryGetParent returns false for a root instead of using a nullable parent value.
if (hierarchy.TryGetParent(platform.Id, out var parent))
{
    Console.WriteLine($"{platform.Name} belongs to {parent.Name}.");
}

Console.WriteLine();
Console.WriteLine("Descendants of Company (DFS is the default):");
PrintNames(hierarchy.Descendants(company.Id));

Console.WriteLine("Descendants of Company (BFS, at most two levels):");
PrintNames(
    hierarchy.Descendants(
        company.Id,
        new SearchOptions
        {
            Traversal = new TraversalOptions
            {
                Strategy = TraversalStrategy.BreadthFirst,
                MaxDepth = 2
            }
        }));

// Move a node together with any descendants it may have.
hierarchy.Move(qualityAssurance.Id, operations.Id);
PrintTree(hierarchy, "After moving Quality assurance to Operations");

// PromoteChildren removes Engineering but keeps Platform by moving it under Company.
hierarchy.Remove(engineering.Id, RemoveBehavior.PromoteChildren);
PrintTree(hierarchy, "After removing Engineering and promoting its children");

// Cascade removes Operations and every descendant under it.
hierarchy.Remove(operations.Id, RemoveBehavior.Cascade);
PrintTree(hierarchy, "After removing Operations with Cascade");

// Replace changes the stored domain object but keeps its position in the hierarchy.
var corePlatform = platform with { Name = "Core platform" };
hierarchy.Replace(platform.Id, corePlatform);
PrintTree(hierarchy, "After replacing Platform");

// Clear removes every node and every relationship.
hierarchy.Clear();
PrintTree(hierarchy, "After Clear");

static void PrintTree(
    Hierarchy<Guid, Department> hierarchy,
    string title)
{
    Console.WriteLine();
    Console.WriteLine(title);

    foreach (var root in hierarchy.GetRoots())
    {
        PrintBranch(hierarchy, root, depth: 0);
    }
}

static void PrintBranch(
    Hierarchy<Guid, Department> hierarchy,
    Department node,
    int depth)
{
    Console.WriteLine($"{new string(' ', depth * 2)}- {node.Name}");

    foreach (var child in hierarchy.GetChildren(node.Id))
    {
        PrintBranch(hierarchy, child, depth + 1);
    }
}

static void PrintNames(IEnumerable<Department> nodes)
{
    Console.WriteLine($"  {string.Join(" → ", nodes.Select(node => node.Name))}");
    Console.WriteLine();
}

public sealed record Department(Guid Id, string Name) : IHierarchyNode<Guid>;
