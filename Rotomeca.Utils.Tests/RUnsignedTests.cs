using System.Numerics;
using Rotomeca.Utils.Types;
using Xunit;

namespace Rotomeca.Utils.Tests;

public class RUnsignedTests
{
    // --- Comportement commun, vérifié sur chaque type -------------------------

    private static void AssertSafeUnsigned<T>() where T : IRUnsignedInteger<T>
    {
        Assert.Throws<OverflowException>(() => T.MaxValue + T.One);
        Assert.Throws<OverflowException>(() => T.Zero - T.One);
        Assert.Throws<OverflowException>(() => T.MaxValue * (T.One + T.One));
        Assert.Throws<OverflowException>(() => -T.One);
        Assert.Throws<OverflowException>(() => { T v = T.Zero; v--; return v; });
        Assert.Throws<OverflowException>(() => T.CreateChecked(-1));
        Assert.Throws<OverflowException>(() => T.CreateTruncating(-1));
        Assert.Throws<OverflowException>(() => T.Parse("-1", null));

        Assert.Equal(T.Zero, T.CreateSaturating(-1));
        Assert.Equal(T.Zero, -T.Zero);
        Assert.Equal(T.MaxValue, T.MaxValue * T.One);
        Assert.Equal(T.MaxValue - T.One, T.MaxValue - T.One);
        Assert.Equal(T.CreateChecked(6), T.CreateChecked(2) * T.CreateChecked(3));
        Assert.Equal(T.CreateChecked(42), T.Parse("42", null));
        Assert.False(T.TryParse("-1", null, out _));
        Assert.Equal(T.AllBitsSet, ~T.Zero);
        Assert.True(T.IsEvenInteger(T.Zero));
        Assert.True(T.IsOddInteger(T.One));
    }

    [Fact] public void RByte_IsSafe() => AssertSafeUnsigned<RByte>();
    [Fact] public void RUshort_IsSafe() => AssertSafeUnsigned<RUshort>();
    [Fact] public void RUint_IsSafe() => AssertSafeUnsigned<RUint>();
    [Fact] public void RUlong_IsSafe() => AssertSafeUnsigned<RUlong>();
    [Fact] public void RUint128_IsSafe() => AssertSafeUnsigned<RUint128>();
    [Fact] public void RNuint_IsSafe() => AssertSafeUnsigned<RNuint>();

    // --- Conversions propres à chaque type --------------------------------------

    [Fact]
    public void Literals_AreImplicit()
    {
        RByte a = 5;
        RUshort b = 5;
        RUlong c = 5;
        RUint128 d = 5;
        RNuint e = 5;

        Assert.Equal((byte)5, a.Value);
        Assert.Equal((ushort)5, b.Value);
        Assert.Equal(5UL, c.Value);
        Assert.Equal((UInt128)5, d.Value);
        Assert.Equal((nuint)5, e.Value);
    }

    [Fact]
    public void ExplicitFromSigned_ChecksBounds()
    {
        Assert.Equal((byte)200, ((RByte)200).Value);
        Assert.Throws<OverflowException>(() => (RByte)256);
        Assert.Throws<OverflowException>(() => (RByte)(-1));
        Assert.Throws<OverflowException>(() => (RUshort)70_000);
        Assert.Throws<OverflowException>(() => (RUlong)(-1L));
        Assert.Throws<OverflowException>(() => (RUint128)Int128.NegativeOne);
        int negative = -1;
        Assert.Throws<OverflowException>(() => (RUint128)negative);
        Assert.Throws<OverflowException>(() => (RUlong)negative);
        Assert.Throws<OverflowException>(() => (RNuint)negative);
        Assert.Throws<OverflowException>(() => (RNuint)(nint)(-1));
        Assert.Equal(ulong.MaxValue >> 1, ((RUlong)long.MaxValue).Value);
    }

    [Fact]
    public void BitwiseOperators_KeepNativeSemantics()
    {
        RByte value = 0b1000_0001;
        Assert.Equal((byte)0b0000_0010, (value << 1).Value);
        Assert.Equal((byte)0b0111_1110, (~value).Value);
        Assert.Equal((byte)0b0100_0000, (value >> 1).Value);

        RUshort word = 0xFF00;
        Assert.Equal((ushort)0x00FF, (word ^ (RUshort)0xFFFF).Value);
    }

    [Fact]
    public void BinaryRoundTrip_Works()
    {
        Span<byte> buffer = stackalloc byte[16];

        RByte b = 0xAB;
        Assert.True(b.TryWriteBigEndian(buffer, out int written));
        Assert.Equal(1, written);
        Assert.Equal(b, RByte.ReadBigEndian(buffer[..1], isUnsigned: true));
        Assert.False(b.TryWriteLittleEndian(Span<byte>.Empty, out written));
        Assert.Equal(0, written);

        RUint128 big = UInt128.MaxValue - 1;
        Assert.True(big.TryWriteLittleEndian(buffer, out written));
        Assert.Equal(16, written);
        Assert.Equal(big, RUint128.ReadLittleEndian(buffer, isUnsigned: true));
    }

    [Fact]
    public void GenericMath_Works()
    {
        static T Sum<T>(IEnumerable<T> values) where T : INumber<T>
        {
            T total = T.Zero;
            foreach (T v in values)
            {
                total += v;
            }

            return total;
        }

        RUlong expected = 6;
        Assert.Equal(expected, Sum(new RUlong[] { 1, 2, 3 }));
        Assert.Throws<OverflowException>(() => Sum(new RByte[] { 200, 100 }));
    }

    // --- Extensions ---------------------------------------------------------------

    [Fact]
    public void Extensions_ConvertToEveryBaseType()
    {
        RByte value = 65;

        Assert.Equal((byte)65, value.ToByte());
        Assert.Equal((sbyte)65, value.ToSByte());
        Assert.Equal((short)65, value.ToInt16());
        Assert.Equal((ushort)65, value.ToUInt16());
        Assert.Equal(65, value.ToInt32());
        Assert.Equal(65U, value.ToUInt32());
        Assert.Equal(65L, value.ToInt64());
        Assert.Equal(65UL, value.ToUInt64());
        Assert.Equal((Int128)65, value.ToInt128());
        Assert.Equal((UInt128)65, value.ToUInt128());
        Assert.Equal((nint)65, value.ToIntPtr());
        Assert.Equal((nuint)65, value.ToUIntPtr());
        Assert.Equal('A', value.ToChar());
        Assert.Equal((Half)65, value.ToHalf());
        Assert.Equal(65f, value.ToSingle());
        Assert.Equal(65d, value.ToDouble());
        Assert.Equal(65m, value.ToDecimal());
    }

    [Fact]
    public void Extensions_ThrowWhenValueDoesNotFit()
    {
        Assert.Throws<OverflowException>(() => ((RByte)200).ToSByte());
        Assert.Throws<OverflowException>(() => RUlong.MaxValue.ToInt32());
        Assert.Throws<OverflowException>(() => RUlong.MaxValue.ToInt64());
        Assert.Throws<OverflowException>(() => RUint.MaxValue.ToChar());
        Assert.Throws<OverflowException>(() => RUint128.MaxValue.ToDecimal());

        Assert.Equal((double)ulong.MaxValue, RUlong.MaxValue.ToDouble());
        Assert.Equal(UInt128.MaxValue, RUint128.MaxValue.ToUInt128());
    }
}
