#if NET8_0_OR_GREATER
using System.Numerics;

namespace Rotomeca.Utils.Types;

/// <summary>
/// Conversions vérifiées des entiers non signés « sûrs » (<see cref="IRUnsignedInteger{TSelf}"/>)
/// vers chaque type numérique de base.
/// </summary>
/// <remarks>
/// Toutes les conversions sont vérifiées : si la valeur ne tient pas dans le type cible, une
/// <see cref="OverflowException"/> est levée au lieu de tronquer silencieusement. Les conversions
/// vers un type à virgule flottante ne lèvent jamais, mais peuvent perdre en précision
/// (<see cref="Half"/>, <see cref="float"/>, <see cref="double"/>).
/// </remarks>
/// <example>
/// <code>
/// RByte petit = 200;
/// int a = petit.ToInt32();            // 200
/// sbyte b = petit.ToSByte();          // OverflowException : 200 &gt; sbyte.MaxValue
///
/// RUlong grand = ulong.MaxValue;
/// double c = grand.ToDouble();        // 1,8446744073709552E+19
/// long d = grand.ToInt64();           // OverflowException
/// </code>
/// </example>
public static class RUnsignedExtensions
{
    /// <summary>Convertit la valeur en <see cref="byte"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="byte"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="byte.MaxValue"/>.</exception>
    public static byte ToByte<T>(this T value) where T : IRUnsignedInteger<T> => byte.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="sbyte"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="sbyte"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="sbyte.MaxValue"/>.</exception>
    public static sbyte ToSByte<T>(this T value) where T : IRUnsignedInteger<T> => sbyte.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="short"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="short"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="short.MaxValue"/>.</exception>
    public static short ToInt16<T>(this T value) where T : IRUnsignedInteger<T> => short.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="ushort"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="ushort"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="ushort.MaxValue"/>.</exception>
    public static ushort ToUInt16<T>(this T value) where T : IRUnsignedInteger<T> => ushort.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="int"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="int"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="int.MaxValue"/>.</exception>
    public static int ToInt32<T>(this T value) where T : IRUnsignedInteger<T> => int.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="uint"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="uint"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="uint.MaxValue"/>.</exception>
    public static uint ToUInt32<T>(this T value) where T : IRUnsignedInteger<T> => uint.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="long"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="long"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="long.MaxValue"/>.</exception>
    public static long ToInt64<T>(this T value) where T : IRUnsignedInteger<T> => long.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="ulong"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="ulong"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="ulong.MaxValue"/>.</exception>
    public static ulong ToUInt64<T>(this T value) where T : IRUnsignedInteger<T> => ulong.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="Int128"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="Int128"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="Int128.MaxValue"/>.</exception>
    public static Int128 ToInt128<T>(this T value) where T : IRUnsignedInteger<T> => Int128.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="UInt128"/>. Toujours valide.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="UInt128"/>.</returns>
    public static UInt128 ToUInt128<T>(this T value) where T : IRUnsignedInteger<T> => UInt128.CreateChecked(value);

    /// <summary>Convertit la valeur en entier signé de taille native (<see cref="nint"/>).</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="nint"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="IntPtr.MaxValue"/>.</exception>
    public static nint ToIntPtr<T>(this T value) where T : IRUnsignedInteger<T> => nint.CreateChecked(value);

    /// <summary>Convertit la valeur en entier non signé de taille native (<see cref="nuint"/>).</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="nuint"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="UIntPtr.MaxValue"/>.</exception>
    public static nuint ToUIntPtr<T>(this T value) where T : IRUnsignedInteger<T> => nuint.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="char"/> (point de code UTF-16).</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>Le caractère dont le code vaut <paramref name="value"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="char.MaxValue"/>.</exception>
    public static char ToChar<T>(this T value) where T : IRUnsignedInteger<T> => CreateChecked<char, T>(value);

    /// <summary>Convertit la valeur en <see cref="Half"/>. Les grandes valeurs deviennent <see cref="Half.PositiveInfinity"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur la plus proche représentable en <see cref="Half"/>.</returns>
    public static Half ToHalf<T>(this T value) where T : IRUnsignedInteger<T> => Half.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="float"/>, avec une éventuelle perte de précision.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur la plus proche représentable en <see cref="float"/>.</returns>
    public static float ToSingle<T>(this T value) where T : IRUnsignedInteger<T> => float.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="double"/>, avec une éventuelle perte de précision.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur la plus proche représentable en <see cref="double"/>.</returns>
    public static double ToDouble<T>(this T value) where T : IRUnsignedInteger<T> => double.CreateChecked(value);

    /// <summary>Convertit la valeur en <see cref="decimal"/>.</summary>
    /// <typeparam name="T">Type entier non signé sûr.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur sous forme de <see cref="decimal"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur dépasse <see cref="decimal.MaxValue"/> (seulement possible depuis <see cref="RUint128"/>).</exception>
    public static decimal ToDecimal<T>(this T value) where T : IRUnsignedInteger<T> => decimal.CreateChecked(value);

    /// <summary>
    /// Accès générique à <c>CreateChecked</c>, implémenté explicitement par certains types
    /// (<see cref="char"/>) et donc inaccessible directement.
    /// </summary>
    private static TTo CreateChecked<TTo, T>(T value)
        where TTo : INumberBase<TTo>
        where T : IRUnsignedInteger<T>
        => TTo.CreateChecked(value);
}
#endif
