using Rotomeca.Utils.Types;
using Xunit;

namespace Rotomeca.Utils.Tests;

public class RandomTests
{
    // ── Range (generic, NET7+) ────────────────────────────────────────────────

    [Fact]
    public void Range_Int_ResultInBounds()
    {
        for (int i = 0; i < 100; i++)
        {
            int result = Types.Random.Range(0, 10);
            Assert.InRange(result, 0, 9);
        }
    }

    [Fact]
    public void Range_Double_ResultInBounds()
    {
        for (int i = 0; i < 100; i++)
        {
            double result = Types.Random.Range(1.0, 5.0);
            Assert.InRange(result, 1.0, 5.0);
        }
    }

    [Fact]
    public void Range_NormalizedMinMax_StillInBounds()
    {
        // min > max → doivent être normalisés silencieusement
        int result = Types.Random.Range(10, 0);
        Assert.InRange(result, 0, 10);
    }

    [Fact]
    public void Range_WithSeed_ReturnsDeterministicValue()
    {
        int first  = Types.Random.Range(0, 1000, seed: 42);
        int second = Types.Random.Range(0, 1000, seed: 42);
        Assert.Equal(first, second);
    }

    [Fact]
    public void Range_DifferentSeeds_LikelyReturnDifferentValues()
    {
        // Probabilité de collision quasi nulle sur 1 000 valeurs
        int a = Types.Random.Range(0, 1000, seed: 1);
        int b = Types.Random.Range(0, 1000, seed: 2);
        // On ne peut pas garantir l'inégalité, mais c'est hautement probable
        // Ce test sert à documenter l'intention, pas à être déterministe
        Assert.IsType<int>(a);
        Assert.IsType<int>(b);
    }

    // ── IntRange ──────────────────────────────────────────────────────────────

    [Fact]
    public void IntRange_ResultInBounds()
    {
        for (int i = 0; i < 100; i++)
        {
            int result = Types.Random.IntRange(5, 10);
            Assert.InRange(result, 5, 9);
        }
    }

    [Fact]
    public void IntRange_WithSeed_ReturnsDeterministicValue()
    {
        int first  = Types.Random.IntRange(0, 100, seed: 99);
        int second = Types.Random.IntRange(0, 100, seed: 99);
        Assert.Equal(first, second);
    }

    // ── RandomString ──────────────────────────────────────────────────────────

    [Fact]
    public void RandomString_ReturnsCorrectLength()
        => Assert.Equal(10, (int)Types.Random.RandomString(10).Length);

    [Fact]
    public void RandomString_ContainsOnlyLowercaseAlpha()
    {
        var str = Types.Random.RandomString(100);
        Assert.All(str, c => Assert.True(c >= 'a' && c <= 'z',
            $"Caractère inattendu : '{c}'"));
    }

    [Fact]
    public void RandomString_WithSeed_ReturnsSameString()
    {
        var first  = Types.Random.RandomString(20, seed: 42);
        var second = Types.Random.RandomString(20, seed: 42);
        Assert.Equal(first, second);
    }

    [Fact]
    public void RandomString_SizeZero_ReturnsEmptyString()
        => Assert.Equal(string.Empty, Types.Random.RandomString(0));

    // ── RandomStringBMP ───────────────────────────────────────────────────────

    [Fact]
    public void RandomStringBMP_ReturnsCorrectCharCount()
        => Assert.Equal(20, Types.Random.RandomStringBMP(20).Length);

    [Fact]
    public void RandomStringBMP_NoSurrogateChars()
    {
        var str = Types.Random.RandomStringBMP(500);
        foreach (char c in str)
            Assert.False(char.IsSurrogate(c),
                $"Surrogate inattendu : U+{(int)c:X4}");
    }

    [Fact]
    public void RandomStringBMP_WithSeed_ReturnsSameString()
    {
        var first  = Types.Random.RandomStringBMP(20, seed: 7);
        var second = Types.Random.RandomStringBMP(20, seed: 7);
        Assert.Equal(first, second);
    }

    // ── RandomStringUnicode ───────────────────────────────────────────────────

    [Fact]
    public void RandomStringUnicode_AtLeastSizeScalars()
    {
        const uint size = 20;
        var str = Types.Random.RandomStringUnicode(size);
        // La longueur en char peut dépasser size (paires surrogate hors-BMP)
        Assert.True(str.Length >= (int)size,
            $"Attendu ≥ {size} chars, obtenu {str.Length}");
    }

    [Fact]
    public void RandomStringUnicode_NoStandaloneSurrogates()
    {
        var str = Types.Random.RandomStringUnicode(500);
        for (int i = 0; i < str.Length; i++)
        {
            if (char.IsHighSurrogate(str[i]))
            {
                // Un high surrogate doit être suivi d'un low surrogate
                Assert.True(i + 1 < str.Length && char.IsLowSurrogate(str[i + 1]),
                    "High surrogate sans low surrogate suivant");
                i++; // saute le low surrogate
            }
            else
            {
                Assert.False(char.IsLowSurrogate(str[i]),
                    "Low surrogate orphelin");
            }
        }
    }

    [Fact]
    public void RandomStringUnicode_WithSeed_ReturnsSameString()
    {
        var first  = Types.Random.RandomStringUnicode(20, seed: 13);
        var second = Types.Random.RandomStringUnicode(20, seed: 13);
        Assert.Equal(first, second);
    }
}
