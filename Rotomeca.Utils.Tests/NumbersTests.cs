using Rotomeca.Utils.Numerics;
using Xunit;

namespace Rotomeca.Utils.Tests;

public class NumbersTests
{
    // ── Clamp ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Clamp_ValueAboveMax_ReturnsMax()
        => Assert.Equal(5, 10.Clamp(0, 5));

    [Fact]
    public void Clamp_ValueBelowMin_ReturnsMin()
        => Assert.Equal(0, (-3).Clamp(0, 5));

    [Fact]
    public void Clamp_ValueInRange_ReturnsUnchanged()
        => Assert.Equal(3, 3.Clamp(0, 5));

    [Fact]
    public void Clamp_ValueEqualToMin_ReturnsMin()
        => Assert.Equal(0, 0.Clamp(0, 5));

    [Fact]
    public void Clamp_ValueEqualToMax_ReturnsMax()
        => Assert.Equal(5, 5.Clamp(0, 5));

    [Fact]
    public void Clamp_Double_ClampedCorrectly()
        => Assert.Equal(1.0, 2.5.Clamp(0.0, 1.0));

    // ── RoundTo ───────────────────────────────────────────────────────────────

    [Fact]
    public void RoundTo_TwoDecimals_RoundsCorrectly()
        => Assert.Equal(3.14, (3.14159).RoundTo(2));

    [Fact]
    public void RoundTo_ZeroDecimals_RoundsToInteger()
        => Assert.Equal(4.0, (3.7).RoundTo(0));

    [Fact]
    public void RoundTo_Float_RoundsCorrectly()
        => Assert.Equal(3.14f, (3.14159f).RoundTo(2));

    [Fact]
    public void RoundTo_DecimalsAbove15_ClampsTo15()
    {
        // Ne doit pas lever d'exception, la valeur de decimals est clampée
        var result = (1.23456789).RoundTo(100);
        Assert.IsType<double>(result);
    }

    // ── IsInRange ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsInRange_ValueInRange_ReturnsTrue()
        => Assert.True(3.IsInRange(1, 5));

    [Fact]
    public void IsInRange_ValueBelowMin_ReturnsFalse()
        => Assert.False(0.IsInRange(1, 5));

    [Fact]
    public void IsInRange_ValueAboveMax_ReturnsFalse()
        => Assert.False(6.IsInRange(1, 5));

    [Fact]
    public void IsInRange_ValueEqualToMin_ReturnsTrue()
        => Assert.True(1.IsInRange(1, 5));

    [Fact]
    public void IsInRange_ValueEqualToMax_ReturnsTrue()
        => Assert.True(5.IsInRange(1, 5));

    [Fact]
    public void IsInRange_Double_WorksCorrectly()
        => Assert.True(2.5.IsInRange(1.0, 5.0));
}
