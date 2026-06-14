using Rotomeca.Core.Collections;
using Rotomeca.Utils.Collections;
using Xunit;

namespace Rotomeca.Utils.Tests;

public class ArraysTests
{
    private static RArray<T> Empty<T>() => new ();

    // ── Chunk ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Chunk_EvenDivision_ReturnsEqualChunks()
    {
        var result = new[] { 1, 2, 3, 4 }.Chunk(2).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal([1, 2], result[0].ToList());
        Assert.Equal([3, 4], result[1].ToList());
    }

    [Fact]
    public void Chunk_OddDivision_LastChunkSmallerThanSize()
    {
        var result = new[] { 1, 2, 3, 4, 5 }.ToRArray().Chunk((uint)2).ToList();
        Assert.Equal(3, result.Count);
        Assert.Single(result[2].ToList()); // dernier chunk = [5]
    }

    [Fact]
    public void Chunk_SizeZero_ReturnsEmpty()
        => Assert.Empty(new[] { 1, 2, 3 }.ToRArray().Chunk((uint)0));

    // ── Unique ────────────────────────────────────────────────────────────────

    [Fact]
    public void Unique_WithDuplicates_RemovesDuplicates()
    {
        var result = new[] { 1, 2, 2, 3, 1, 4 }.Unique().ToList();
        Assert.Equal(4, result.Count);
        Assert.Equal([1, 2, 3, 4], result);
    }

    [Fact]
    public void Unique_NoDuplicates_ReturnsSameElements()
    {
        var result = new[] { 1, 2, 3 }.Unique().ToList();
        Assert.Equal(3, result.Count);
    }

    // ── UniqueBy ──────────────────────────────────────────────────────────────

    [Fact]
    public void UniqueBy_DuplicateKey_KeepsFirstOccurrence()
    {
        var people = new[]
        {
            (Name: "Alice", Age: 30),
            (Name: "Bob",   Age: 25),
            (Name: "Alice", Age: 42), // doublon sur Name
        };

        var result = people.UniqueBy(p => p.Name).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(30, result.First(p => p.Name == "Alice").Age); // première occurrence conservée
    }

    // ── GroupTo ───────────────────────────────────────────────────────────────

    [Fact]
    public void GroupTo_GroupsByKey_ReturnsCorrectGroups()
    {
        var words = new[] { "apple", "avocado", "banana", "blueberry" };
        var groups = words.GroupTo(w => w[0]);

        Assert.True(groups.ContainsKey('a'));
        Assert.True(groups.ContainsKey('b'));
        Assert.Equal(2, groups['a'].Count());
        Assert.Equal(2, groups['b'].Count());
    }

    private static readonly int[] values = [10, 20, 30];

    // ── First / Last ──────────────────────────────────────────────────────────

    [Fact]
    public void First_NonEmptySequence_ReturnsFirstElement()
    {
        var result = new RArray<int>(values).First();
        Assert.True(result.HasValue);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void First_EmptySequence_ReturnsNull()
    {
        var result = Empty<int?>().First();
        Assert.False(result.HasValue);
    }

    [Fact]
    public void Last_NonEmptySequence_ReturnsLastElement()
    {
        var result = new[] { 10, 20, 30 }.ToRArray().Last();
        Assert.True(result.HasValue);
        Assert.Equal(30, result.Value);
    }

    [Fact]
    public void Last_EmptySequence_ReturnsNull()
    {
        var result = Array.Empty<int>().ToRArray().Last();
        Assert.False(result.HasValue);
    }

    // ── Sum ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Sum_Ints_ReturnsCorrectSum()
        => Assert.Equal(10, new[] { 1, 2, 3, 4 }.Sum());

    [Fact]
    public void Sum_EmptySequence_ReturnsZero()
        => Assert.Equal(0, Array.Empty<int>().Sum());

    [Fact]
    public void Sum_Doubles_ReturnsCorrectSum()
        => Assert.Equal(6.0, new[] { 1.0, 2.0, 3.0 }.Sum());

    // ── SortBy ────────────────────────────────────────────────────────────────

    [Fact]
    public void SortBy_ByName_ReturnsSortedAscending()
    {
        var people = new[]
        {
            (Name: "Charlie", Age: 35),
            (Name: "Alice",   Age: 28),
            (Name: "Bob",     Age: 22),
        };

        var sorted = people.SortBy(p => p.Name).ToList();

        Assert.Equal("Alice",   sorted[0].Name);
        Assert.Equal("Bob",     sorted[1].Name);
        Assert.Equal("Charlie", sorted[2].Name);
    }

    // ── Flatten ───────────────────────────────────────────────────────────────

    [Fact]
    public void Flatten_NestedArrays_ReturnsFlat()
    {
        var nested = new[] { new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5 } };
        var flat = nested.Flatten().ToList();

        Assert.Equal([1, 2, 3, 4, 5], flat);
    }

    // ── FlattenDeep ───────────────────────────────────────────────────────────

    [Fact]
    public void FlattenDeep_DeeplyNested_ReturnsAllLeafValues()
    {
        object[] nested = [1, new object[] { 2, new object[] { 3, 4 } }, 5];
        var flat = nested.FlattenDeep<int>().ToList();

        Assert.Equal([1, 2, 3, 4, 5], flat);
    }

    // ── Compact ───────────────────────────────────────────────────────────────

    [Fact]
    public void Compact_WithNulls_RemovesNulls()
    {
        var result = new string?[] { "hello", null, "world", null }
            .Compact()
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(null, result);
    }

    // ── Partition ─────────────────────────────────────────────────────────────

    [Fact]
    public void Partition_ByParity_SeparatesEvenAndOdd()
    {
        var (evens, odds) = new[] { 1, 2, 3, 4, 5, 6 }.Partition(n => n % 2 == 0);

        Assert.Equal([2, 4, 6], evens.ToList());
        Assert.Equal([1, 3, 5], odds.ToList());
    }

    // ── Intersection ─────────────────────────────────────────────────────────

    [Fact]
    public void Intersection_OverlappingArrays_ReturnsCommonElements()
    {
        var result = new[] { 1, 2, 3, 4 }.Intersection(new[] { 3, 4, 5, 6 }).ToList();
        Assert.Equal([3, 4], result);
    }

    // ── Difference ───────────────────────────────────────────────────────────

    [Fact]
    public void Difference_OverlappingArrays_ReturnsElementsNotInOther()
    {
        var result = new[] { 1, 2, 3, 4 }.Difference(new[] { 3, 4 }).ToList();
        Assert.Equal([1, 2], result);
    }

    // ── Union ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Union_OverlappingArrays_ReturnsMergedWithoutDuplicates()
    {
        var result = new[] { 1, 2, 3 }.ToRArray().Union(new[] { 3, 4, 5 }).ToList();
        Assert.Equal(5, result.Count);
        Assert.DoesNotContain(result, r => result.Count(x => x == r) > 1);
    }

    // ── Take / Drop ───────────────────────────────────────────────────────────

    [Fact]
    public void Take_ReturnsFirstNElements()
    {
        var result = new[] { 1, 2, 3, 4, 5 }.ToRArray().Take(3u).ToList();
        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public void Drop_SkipsFirstNElements()
    {
        var result = new[] { 1, 2, 3, 4, 5 }.Drop(2u).ToList();
        Assert.Equal([3, 4, 5], result);
    }

    [Fact]
    public void Take_MoreThanLength_ReturnsAll()
    {
        var result = new[] { 1, 2 }.ToRArray().Take(100u).ToList();
        Assert.Equal([1, 2], result);
    }

    // ── Zip ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Zip_SameLength_ReturnsPairs()
    {
        var result = new[] { 1, 2, 3 }.ToRArray().Zip(new[] { "a", "b", "c" }).ToList();
        Assert.Equal(3, result.Count);
        Assert.Equal((1, "a"), result[0]);
        Assert.Equal((3, "c"), result[2]);
    }

    [Fact]
    public void Zip_DifferentLength_StopsAtShortest()
    {
        var result = new[] { 1, 2, 3 }.ToRArray().Zip(new[] { "a", "b" }).ToList();
        Assert.Equal(2, result.Count);
    }

    // ── Shuffle ───────────────────────────────────────────────────────────────

    [Fact]
    public void Shuffle_RetainsSameElements()
    {
        var original = new[] { 1, 2, 3, 4, 5 };
        var shuffled = original.Shuffle().ToList();

        Assert.Equal(5, shuffled.Count);
        Assert.All(original, x => Assert.Contains(x, shuffled));
    }

    // ── MinBy / MaxBy ─────────────────────────────────────────────────────────

    [Fact]
    public void MinBy_ReturnsElementWithSmallestKey()
    {
        var people = new[]
        {
            (Name: "Alice", Age: 30L),
            (Name: "Bob",   Age: 25L),
            (Name: "Carol", Age: 40L),
        };

        var youngest = people.MinBy(p => p.Age);
        Assert.True(youngest.HasValue);
        Assert.Equal("Bob", youngest.Value.Name);
    }

    [Fact]
    public void MaxBy_ReturnsElementWithLargestKey()
    {
        var people = new[]
        {
            (Name: "Alice", Age: 30L),
            (Name: "Bob",   Age: 25L),
            (Name: "Carol", Age: 40L),
        };

        var oldest = people.MaxBy(p => p.Age);
        Assert.True(oldest.HasValue);
        Assert.Equal("Carol", oldest.Value.Name);
    }

    [Fact]
    public void MinBy_EmptySequence_ReturnsNull()
    {
        var result = Array.Empty<(string Name, long Age)>().MinBy(p => p.Age);
        Assert.False(result.HasValue);
    }
}
