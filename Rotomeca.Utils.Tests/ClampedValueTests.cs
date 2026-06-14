using Rotomeca.Utils.Types;
using Xunit;

namespace Rotomeca.Utils.Tests;

public class ClampedValueTests
{
    // ── Construction ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ValueInRange_StoresValueUnchanged()
    {
        var clamped = new ClampedValue<int>(0, 100, 50);
        Assert.Equal(50, clamped.Value);
    }

    [Fact]
    public void Constructor_ValueAboveMax_ClampsToMax()
    {
        var clamped = new ClampedValue<int>(0, 100, 150);
        Assert.Equal(100, clamped.Value);
    }

    [Fact]
    public void Constructor_ValueBelowMin_ClampsToMin()
    {
        var clamped = new ClampedValue<int>(0, 100, -10);
        Assert.Equal(0, clamped.Value);
    }

    [Fact]
    public void Constructor_MinGreaterThanMax_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() => new ClampedValue<int>(10, 5, 7));

    [Fact]
    public void Constructor_MinEqualsMax_AcceptsExactValue()
    {
        var clamped = new ClampedValue<int>(5, 5, 5);
        Assert.Equal(5, clamped.Value);
    }

    [Fact]
    public void Constructor_StoresMinAndMax()
    {
        var clamped = new ClampedValue<int>(0, 100, 50);
        Assert.Equal(0,   clamped.Min);
        Assert.Equal(100, clamped.Max);
    }

    // ── WithValue ─────────────────────────────────────────────────────────────

    [Fact]
    public void WithValue_NewInRangeValue_ReturnsUpdatedInstance()
    {
        var original = new ClampedValue<int>(0, 100, 50);
        var updated  = original.WithValue(80);
        Assert.Equal(80, updated.Value);
    }

    [Fact]
    public void WithValue_ValueAboveMax_ClampsToMax()
    {
        var original = new ClampedValue<int>(0, 100, 50);
        var updated  = original.WithValue(200);
        Assert.Equal(100, updated.Value);
    }

    [Fact]
    public void WithValue_DoesNotMutateOriginal()
    {
        var original = new ClampedValue<int>(0, 100, 50);
        original.WithValue(80);
        Assert.Equal(50, original.Value);
    }

    // ── Implicit conversion ───────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversion_ToT_ReturnsValue()
    {
        var clamped = new ClampedValue<int>(0, 100, 42);
        int raw = clamped;
        Assert.Equal(42, raw);
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void Equals_SameMinMaxValue_ReturnsTrue()
    {
        var a = new ClampedValue<int>(0, 100, 50);
        var b = new ClampedValue<int>(0, 100, 50);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var a = new ClampedValue<int>(0, 100, 50);
        var b = new ClampedValue<int>(0, 100, 60);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void EqualityOperator_SameValues_ReturnsTrue()
    {
        var a = new ClampedValue<int>(0, 100, 50);
        var b = new ClampedValue<int>(0, 100, 50);
        Assert.True(a == b);
    }

    [Fact]
    public void InequalityOperator_DifferentValues_ReturnsTrue()
    {
        var a = new ClampedValue<int>(0, 100, 50);
        var b = new ClampedValue<int>(0, 100, 60);
        Assert.True(a != b);
    }

    // ── Deconstruct ───────────────────────────────────────────────────────────

    [Fact]
    public void Deconstruct_ReturnsCorrectComponents()
    {
        var clamped = new ClampedValue<int>(0, 100, 42);
        var (min, max, value) = clamped;
        Assert.Equal(0,   min);
        Assert.Equal(100, max);
        Assert.Equal(42,  value);
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var clamped = new ClampedValue<int>(0, 100, 42);
        Assert.Equal("[0, 100] = 42", clamped.ToString());
    }

    // ── Double type ───────────────────────────────────────────────────────────

    [Fact]
    public void ClampedValue_Double_WorksCorrectly()
    {
        var clamped = new ClampedValue<double>(0.0, 1.0, 1.5);
        Assert.Equal(1.0, clamped.Value);
    }
}
