using Rotomeca.Utils.Dictionaries;
using Xunit;

namespace Rotomeca.Utils.Tests;

public class DictsTests
{
    // ── IsEmpty ───────────────────────────────────────────────────────────────

    [Fact]
    public void IsEmpty_EmptyDictionary_ReturnsTrue()
        => Assert.True(new Dictionary<string, int>().IsEmpty());

    [Fact]
    public void IsEmpty_NonEmptyDictionary_ReturnsFalse()
        => Assert.False(new Dictionary<string, int> { ["a"] = 1 }.IsEmpty());

    // ── Pick ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Pick_ExistingKeys_ReturnsSubset()
    {
        IDictionary<string, int> source = new Dictionary<string, int>
            { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        var result = source.Pick("a", "c");

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result["a"]);
        Assert.Equal(3, result["c"]);
        Assert.False(result.ContainsKey("b"));
    }

    [Fact]
    public void Pick_EmptyKeys_ReturnsEmptyDictionary()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1 };
        var result = source.Pick();
        Assert.Empty(result);
    }

    [Fact]
    public void Pick_AbsentKey_IgnoresSilently()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1 };
        var result = source.Pick("a", "z");
        Assert.Single(result);
        Assert.Equal(1, result["a"]);
    }

    // ── Omit ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Omit_ExistingKeys_ExcludesKeys()
    {
        IDictionary<string, int> source = new Dictionary<string, int>
            { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        var result = source.Omit("b");

        Assert.Equal(2, result.Count);
        Assert.False(result.ContainsKey("b"));
        Assert.Equal(1, result["a"]);
        Assert.Equal(3, result["c"]);
    }

    [Fact]
    public void Omit_EmptyKeys_ReturnsCopy()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1 };
        var result = source.Omit();
        Assert.Equal(source.Count, result.Count);
        Assert.Equal(1, result["a"]);
    }

    [Fact]
    public void Omit_AbsentKey_NoEffect()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 1 };
        var result = source.Omit("z");
        Assert.Single(result);
    }

    // ── DeepMerge ─────────────────────────────────────────────────────────────

    [Fact]
    public void DeepMerge_NonConflicting_MergesBothDictionaries()
    {
        Dictionary<string, int> target = new Dictionary<string, int> { ["a"] = 1 };
        Dictionary<string, int> source = new Dictionary<string, int> { ["b"] = 2 };

        var result = target.DeepMerge(source);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result["a"]);
        Assert.Equal(2, result["b"]);
    }

    [Fact]
    public void DeepMerge_KeyConflict_SourceValueWins()
    {
        IDictionary<string, int> target = new Dictionary<string, int> { ["a"] = 1 };
        IDictionary<string, int> source = new Dictionary<string, int> { ["a"] = 99 };

        var result = target.DeepMerge(source);

        Assert.Equal(99, result["a"]);
    }

    [Fact]
    public void DeepMerge_DoesNotMutateTarget()
    {
        IDictionary<string, int> target = new Dictionary<string, int> { ["a"] = 1 };
        IDictionary<string, int> source = new Dictionary<string, int> { ["b"] = 2 };

        target.DeepMerge(source);

        Assert.Single(target);
        Assert.False(target.ContainsKey("b"));
    }

    [Fact]
    public void DeepMerge_NestedDictionaries_MergesRecursively()
    {
        IDictionary<string, IDictionary<string, int>> target =
            new Dictionary<string, IDictionary<string, int>>
            {
                ["nested"] = new Dictionary<string, int> { ["x"] = 1 }
            };

        IDictionary<string, IDictionary<string, int>> source =
            new Dictionary<string, IDictionary<string, int>>
            {
                ["nested"] = new Dictionary<string, int> { ["y"] = 2 }
            };

        var result = target.DeepMerge(source);
        var nested = result["nested"];

        Assert.Equal(2, nested.Count);
        Assert.Equal(1, nested["x"]);
        Assert.Equal(2, nested["y"]);
    }

    // ── FlattenObject ─────────────────────────────────────────────────────────

    [Fact]
    public void FlattenObject_SingleLevel_ReturnsSameKeys()
    {
        IDictionary<string, object?> source = new Dictionary<string, object?>
            { ["a"] = 1, ["b"] = "hello" };

        var result = source.FlattenObject();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result["a"]);
        Assert.Equal("hello", result["b"]);
    }

    [Fact]
    public void FlattenObject_Nested_ReturnsDotSeparatedKeys()
    {
        IDictionary<string, object?> source = new Dictionary<string, object?>
        {
            ["a"] = new Dictionary<string, object?>
            {
                ["b"] = new Dictionary<string, object?>
                {
                    ["c"] = 42
                }
            }
        };

        var result = source.FlattenObject();

        Assert.Single(result);
        Assert.Equal(42, result["a.b.c"]);
    }

    [Fact]
    public void FlattenObject_CustomSeparator_UsesCorrectSeparator()
    {
        IDictionary<string, object?> source = new Dictionary<string, object?>
        {
            ["a"] = new Dictionary<string, object?> { ["b"] = 1 }
        };

        var result = source.FlattenObject(separator: "_");

        Assert.True(result.ContainsKey("a_b"));
        Assert.False(result.ContainsKey("a.b"));
    }

    // ── UnflattenObject ───────────────────────────────────────────────────────

    [Fact]
    public void UnflattenObject_DotKey_RebuildsNested()
    {
        IDictionary<string, object?> flat = new Dictionary<string, object?>
            { ["a.b.c"] = 42 };

        var result = (IDictionary<string, object?>)flat.UnflattenObject();

        Assert.True(result.ContainsKey("a"));
        var a = (IDictionary<string, object?>)result["a"]!;
        var b = (IDictionary<string, object?>)a["b"]!;
        Assert.Equal(42, b["c"]);
    }

    [Fact]
    public void FlattenObject_ThenUnflatten_RoundTrip()
    {
        IDictionary<string, object?> source = new Dictionary<string, object?>
        {
            ["x"] = new Dictionary<string, object?> { ["y"] = 99 },
            ["z"] = "hello"
        };

        var flat    = source.FlattenObject();
        var rebuilt = (IDictionary<string, object?>)flat.UnflattenObject();

        Assert.True(rebuilt.ContainsKey("x"));
        Assert.Equal("hello", rebuilt["z"]);
    }

    // ── FilterKeys ────────────────────────────────────────────────────────────

    [Fact]
    public void FilterKeys_Predicate_KeepsMatchingKeys()
    {
        IDictionary<string, int> source = new Dictionary<string, int>
            { ["a"] = 1, ["b"] = 2, ["ab"] = 3 };

        var result = source.FilterKeys(k => k.StartsWith("a"));

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("a"));
        Assert.True(result.ContainsKey("ab"));
        Assert.False(result.ContainsKey("b"));
    }

    // ── FilterValues ──────────────────────────────────────────────────────────

    [Fact]
    public void FilterValues_Predicate_KeepsMatchingValues()
    {
        IDictionary<string, int> source = new Dictionary<string, int>
            { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        var result = source.FilterValues(v => v > 1);

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("b"));
        Assert.True(result.ContainsKey("c"));
        Assert.False(result.ContainsKey("a"));
    }

    // ── MapValues ─────────────────────────────────────────────────────────────

    [Fact]
    public void MapValues_Transform_TransformsAllValues()
    {
        IDictionary<string, int> source = new Dictionary<string, int>
            { ["a"] = 1, ["b"] = 2 };

        var result = source.MapValues((v, _) => v * 10);

        Assert.Equal(10, result["a"]);
        Assert.Equal(20, result["b"]);
    }

    [Fact]
    public void MapValues_KeyPassedToFn_KeyIsCorrect()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { ["key"] = 0 };
        string? capturedKey = null;

        source.MapValues((_, k) => { capturedKey = k; return 0; });

        Assert.Equal("key", capturedKey);
    }

    // ── MapKeys ───────────────────────────────────────────────────────────────

    [Fact]
    public void MapKeys_Transform_TransformsAllKeys()
    {
        IDictionary<string, int> source = new Dictionary<string, int>
            { ["a"] = 1, ["b"] = 2 };

        var result = source.MapKeys((k, _) => k.ToUpper());

        Assert.True(result.ContainsKey("A"));
        Assert.True(result.ContainsKey("B"));
        Assert.Equal(1, result["A"]);
    }

    // ── Invert ────────────────────────────────────────────────────────────────

    [Fact]
    public void Invert_UniqueValues_InvertsKeysAndValues()
    {
        IDictionary<string, int> source = new Dictionary<string, int>
            { ["a"] = 1, ["b"] = 2 };

        var result = source.Invert();

        Assert.Equal("a", result[1]);
        Assert.Equal("b", result[2]);
    }

    [Fact]
    public void Invert_DuplicateValues_ThrowsArgumentException()
    {
        IDictionary<string, int> source = new Dictionary<string, int>
            { ["a"] = 1, ["b"] = 1 };

        Assert.Throws<ArgumentException>(() => source.Invert());
    }

    // ── DeepClone ─────────────────────────────────────────────────────────────

    [Fact]
    public void DeepClone_PrimitiveType_ReturnsEqualValue()
    {
        const int value = 42;
        var clone = value.DeepClone();
        Assert.Equal(value, clone);
    }

    [Fact]
    public void DeepClone_ReferenceType_ReturnsDifferentReference()
    {
        var original = new List<int> { 1, 2, 3 };
        var clone = original.DeepClone();

        Assert.NotSame(original, clone);
        Assert.Equal(original, clone);
    }

    [Fact]
    public void DeepClone_Mutation_DoesNotAffectOriginal()
    {
        var original = new List<int> { 1, 2, 3 };
        var clone = original.DeepClone();
        clone.Add(99);

        Assert.Equal(3, original.Count);
        Assert.Equal(4, clone.Count);
    }
}
