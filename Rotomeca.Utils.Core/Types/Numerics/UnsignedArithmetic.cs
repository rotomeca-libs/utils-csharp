#if NET8_0_OR_GREATER
using System.Numerics;

namespace Rotomeca.Utils.Types;

/// <summary>
/// Arithmétique vérifiée partagée par les entiers non signés « sûrs ». Les opérations sont
/// génériques mais spécialisées par le JIT pour chaque type primitif, sans coût à l'exécution.
/// </summary>
internal static class UnsignedArithmetic
{
    /// <summary>Additionne deux valeurs en vérifiant le dépassement.</summary>
    /// <typeparam name="T">Type non signé sous-jacent.</typeparam>
    /// <param name="left">Premier terme.</param>
    /// <param name="right">Second terme.</param>
    /// <returns>La somme.</returns>
    /// <exception cref="OverflowException">Si la somme dépasse <c>T.MaxValue</c>.</exception>
    public static T Add<T>(T left, T right)
        where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        => left > T.MaxValue - right
            ? throw new OverflowException($"Opération invalide : {left} + {right} dépasse {T.MaxValue}.")
            : left + right;

    /// <summary>Soustrait deux valeurs en refusant un résultat négatif.</summary>
    /// <typeparam name="T">Type non signé sous-jacent.</typeparam>
    /// <param name="left">Valeur de départ.</param>
    /// <param name="right">Valeur à soustraire.</param>
    /// <param name="typeName">Nom du type appelant, pour le message d'erreur.</param>
    /// <returns>La différence.</returns>
    /// <exception cref="OverflowException">Si <paramref name="right"/> est supérieur à <paramref name="left"/>.</exception>
    public static T Subtract<T>(T left, T right, string typeName)
        where T : IBinaryInteger<T>, IUnsignedNumber<T>
        => right > left
            ? throw new OverflowException($"Opération invalide : {left} - {right} est négatif, un {typeName} ne peut pas l'être.")
            : left - right;

    /// <summary>Multiplie deux valeurs en vérifiant le dépassement.</summary>
    /// <typeparam name="T">Type non signé sous-jacent.</typeparam>
    /// <param name="left">Premier facteur.</param>
    /// <param name="right">Second facteur.</param>
    /// <returns>Le produit.</returns>
    /// <exception cref="OverflowException">Si le produit dépasse <c>T.MaxValue</c>.</exception>
    public static T Multiply<T>(T left, T right)
        where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
        => !T.IsZero(left) && right > T.MaxValue / left
            ? throw new OverflowException($"Opération invalide : {left} * {right} dépasse {T.MaxValue}.")
            : left * right;

    /// <summary>Convertit un entier signé en entier non signé en vérifiant les deux bornes.</summary>
    /// <typeparam name="TSigned">Type signé source.</typeparam>
    /// <typeparam name="T">Type non signé cible.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <param name="typeName">Nom du type appelant, pour le message d'erreur.</param>
    /// <returns>La valeur convertie.</returns>
    /// <exception cref="OverflowException">Si <paramref name="value"/> est négatif ou dépasse <c>T.MaxValue</c>.</exception>
    public static T FromSigned<TSigned, T>(TSigned value, string typeName)
        where TSigned : IBinaryInteger<TSigned>, ISignedNumber<TSigned>
        where T : IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        if (TSigned.IsNegative(value))
        {
            throw new OverflowException($"Opération invalide : la valeur {value} est négative, un {typeName} ne peut pas l'être.");
        }

        // Tous les types concernés tiennent dans un UInt128 : comparaison sans perte.
        return UInt128.CreateTruncating(value) > UInt128.CreateTruncating(T.MaxValue)
            ? throw new OverflowException($"Opération invalide : la valeur {value} dépasse {T.MaxValue}.")
            : T.CreateTruncating(value);
    }
}
#endif
