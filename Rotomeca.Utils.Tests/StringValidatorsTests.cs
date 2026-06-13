using Rotomeca.Utils.Validators;
using Xunit;

namespace Rotomeca.Utils.Tests;

public class StringValidatorsTests
{
    // ── IsEmail ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("user@example.com",          true)]
    [InlineData("user.name+tag@domain.co.uk", true)]
    [InlineData("a@b.c",                      true)]
    [InlineData("invalid",                    false)]
    [InlineData("@no-user.com",               false)]
    [InlineData("no-domain@",                 false)]
    [InlineData("no-at-sign.com",             false)]
    [InlineData("spaces @domain.com",         false)]
    [InlineData("",                           false)]
    public void IsEmail_VariousCases_ReturnsExpected(string input, bool expected)
        => Assert.Equal(expected, input.IsEmail());

    // ── IsUrl ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("https://example.com",             true)]
    [InlineData("http://localhost:3000/path?q=1",  true)]
    [InlineData("http://192.168.1.1/api",          true)]
    [InlineData("ftp://example.com",               false)] // schéma non http/https
    [InlineData("//relative.com",                  false)] // relatif
    [InlineData("not-a-url",                       false)]
    [InlineData("",                                false)]
    public void IsUrl_VariousCases_ReturnsExpected(string input, bool expected)
        => Assert.Equal(expected, input.IsUrl());

    // ── IsUUID ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000", true)]
    [InlineData("550E8400-E29B-41D4-A716-446655440000", true)]  // majuscules
    [InlineData("not-a-uuid",                           false)]
    [InlineData("550e8400-e29b-41d4-a716",              false)] // tronqué
    [InlineData("550e8400e29b41d4a716446655440000",     false)] // sans tirets
    [InlineData("",                                     false)]
    public void IsUUID_VariousCases_ReturnsExpected(string input, bool expected)
        => Assert.Equal(expected, input.IsUUID());

    // ── IsNumeric ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("42",     true)]
    [InlineData("3.14",   true)]   // point décimal (InvariantCulture)
    [InlineData("-10",    true)]
    [InlineData("0",      true)]
    [InlineData("3,14",   false)]  // virgule rejetée par InvariantCulture
    [InlineData("abc",    false)]
    [InlineData("1e5",    true)]   // notation scientifique
    [InlineData("",       false)]
    public void IsNumeric_VariousCases_ReturnsExpected(string input, bool expected)
        => Assert.Equal(expected, input.IsNumeric());

    [Fact]
    public void IsNumeric_CommaDecimalSeparator_ReturnsFalse()
        => Assert.False("3,14".IsNumeric()); // InvariantCulture n'accepte pas la virgule

    // ── IsAlpha ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("hello",       true)]
    [InlineData("Bonjour",     true)]
    [InlineData("éàüÀÿ",      true)]   // accentués (plage À-ÿ)
    [InlineData("hello123",    false)]
    [InlineData("hello world", false)] // espace
    [InlineData("hello!",      false)]
    [InlineData("",            false)]
    public void IsAlpha_VariousCases_ReturnsExpected(string input, bool expected)
        => Assert.Equal(expected, input.IsAlpha());

    // ── IsHexColor ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("#fff",    true)]
    [InlineData("#FFF",    true)]
    [InlineData("#ffffff", true)]
    [InlineData("#FFFFFF", true)]
    [InlineData("fff",     true)]   // sans #
    [InlineData("ffffff",  true)]   // sans #
    [InlineData("#ffff",   false)]  // 4 chars invalide
    [InlineData("#fffff",  false)]  // 5 chars invalide
    [InlineData("#xyz",    false)]  // caractères non hex
    [InlineData("#gggggg", false)]
    [InlineData("",        false)]
    public void IsHexColor_VariousCases_ReturnsExpected(string input, bool expected)
        => Assert.Equal(expected, input.IsHexColor());
}
