using HierarchyKit.Hierarchy;

namespace HierarchyKit.Tests.Hierarchy;

public sealed class TreeRelationsTests
{
    [Fact]
    public void Move_ShouldUseConfiguredComparer_WhenRemovingFromOldParent()
    {
        var relations = new TreeRelations<string>(StringComparer.OrdinalIgnoreCase);
        relations.Add("Root", "Child");
        relations.Add("Other", "Sibling");

        relations.Move("child", "OTHER");

        Assert.Empty(relations.GetChildren("root"));
        Assert.Equal(new[] { "Sibling", "Child" }, relations.GetChildren("other"));
        Assert.True(relations.TryGetParent("CHILD", out var parent));
        Assert.Equal("OTHER", parent);
    }

    [Fact]
    public void Remove_ShouldUseConfiguredComparer_WhenRemovingFromParent()
    {
        var relations = new TreeRelations<string>(StringComparer.OrdinalIgnoreCase);
        relations.Add("Root", "Child");

        relations.Remove("root", "CHILD");

        Assert.Empty(relations.GetChildren("ROOT"));
        Assert.False(relations.TryGetParent("child", out _));
    }

    [Fact]
    public void Add_ShouldLeaveRelationsUnchanged_WhenComparerThrows()
    {
        var comparer = new ThrowingComparer();
        var relations = new TreeRelations<string>(comparer);
        relations.Add("Root", "Existing");

        comparer.ThrowOnCall = 1;

        Assert.Throws<InvalidOperationException>(
            () => relations.Add("Root", "New"));

        comparer.ThrowOnCall = null;

        Assert.Equal(new[] { "Existing" }, relations.GetChildren("Root"));
        Assert.False(relations.TryGetParent("New", out _));
    }

    [Fact]
    public void Move_ShouldLeaveRelationsUnchanged_WhenComparerThrowsDuringPreparation()
    {
        var comparer = new ThrowingComparer();
        var relations = new TreeRelations<string>(comparer);
        relations.Add("Old", "Child");
        relations.Add("New", "Existing");

        comparer.ThrowOnCall = 1;

        Assert.Throws<InvalidOperationException>(
            () => relations.Move("Child", "New"));

        comparer.ThrowOnCall = null;

        Assert.Equal(new[] { "Child" }, relations.GetChildren("Old"));
        Assert.Equal(new[] { "Existing" }, relations.GetChildren("New"));
        Assert.True(relations.TryGetParent("Child", out var parent));
        Assert.Equal("Old", parent);
    }

    [Fact]
    public void Remove_ShouldLeaveRelationsUnchanged_WhenComparerThrowsDuringPreparation()
    {
        var comparer = new ThrowingComparer();
        var relations = new TreeRelations<string>(comparer);
        relations.Add("Root", "Child");

        comparer.ThrowOnCall = 1;

        Assert.Throws<InvalidOperationException>(
            () => relations.Remove("Root", "Child"));

        comparer.ThrowOnCall = null;

        Assert.Equal(new[] { "Child" }, relations.GetChildren("Root"));
        Assert.True(relations.TryGetParent("Child", out var parent));
        Assert.Equal("Root", parent);
    }

    private sealed class ThrowingComparer : IEqualityComparer<string>
    {
        private int _callCount;
        private int? _throwOnCall;

        public int? ThrowOnCall
        {
            get => _throwOnCall;
            set
            {
                _throwOnCall = value;
                _callCount = 0;
            }
        }

        public bool Equals(string? x, string? y)
        {
            ThrowIfRequired();
            return StringComparer.OrdinalIgnoreCase.Equals(x, y);
        }

        public int GetHashCode(string obj)
        {
            ThrowIfRequired();
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
        }

        private void ThrowIfRequired()
        {
            _callCount++;

            if (_throwOnCall == _callCount)
            {
                throw new InvalidOperationException("Injected comparer failure.");
            }
        }
    }
}
