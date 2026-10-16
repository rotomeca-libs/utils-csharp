#if NET8_0_OR_GREATER
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace Rotomeca.Utils.Types;

/// <summary>
/// Entier non signé de taille native (32 bits ou 64 bits selon la plateforme) qui se comporte comme un <see cref="nuint"/>, mais qui lève une
/// <see cref="OverflowException"/> au lieu de boucler silencieusement lorsqu'une opération
/// produirait une valeur négative ou supérieure à <see cref="nuint.MaxValue"/>.
/// </summary>
/// <param name="Value">Valeur brute sous-jacente.</param>
/// <remarks>
/// <para>
/// <see cref="RNuint"/> implémente <see cref="IBinaryInteger{TSelf}"/> et
/// <see cref="IUnsignedNumber{TSelf}"/> : il est donc utilisable dans tout code générique
/// contraint sur <see cref="INumber{TSelf}"/>, <see cref="INumberBase{TSelf}"/>,
/// <see cref="IBinaryInteger{TSelf}"/>, etc.
/// </para>
/// <para>
/// Règle de sécurité : toute opération dont le résultat mathématique serait négatif
/// (soustraction, décrémentation, négation, conversion depuis un type signé, parsing de
/// <c>"-1"</c>…) lève une <see cref="OverflowException"/>. Il en va de même pour un dépassement
/// de <see cref="nuint.MaxValue"/>. Le contexte <see langword="unchecked"/> ne réactive pas
/// le bouclage.
/// </para>
/// <para>
/// Exceptions volontaires : les opérateurs bit à bit (<c>&amp;</c>, <c>|</c>, <c>^</c>,
/// <c>~</c>) et les décalages (<c>&lt;&lt;</c>, <c>&gt;&gt;</c>, <c>&gt;&gt;&gt;</c>)
/// conservent la sémantique de <see cref="nuint"/> (bits perdus sans exception), car ils ne
/// peuvent pas produire de valeur négative. <see cref="CreateSaturating{TOther}"/> ramène
/// les valeurs négatives à 0, conformément à son contrat.
/// </para>
/// <para>
/// Limite : un cast explicite vers <see cref="nuint"/> effectué <em>avant</em> la création
/// (<c>new RNuint((nuint)entierNegatif)</c>) boucle en dehors de <see cref="RNuint"/>, qui
/// reçoit alors une valeur positive valide. Pour convertir une valeur signée, utiliser
/// <c>(RNuint)valeur</c> ou <see cref="CreateChecked{TOther}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// RNuint stock = 10;            // conversion implicite depuis un littéral
/// stock -= 3;                  // 7
/// nuint brut = stock;           // conversion implicite vers nuint
///
/// stock -= 8;                  // OverflowException : le résultat serait -1
///
/// int saisie = -2;
/// RNuint a = (RNuint)saisie;               // OverflowException
/// RNuint b = RNuint.CreateChecked(saisie); // OverflowException
/// RNuint c = RNuint.CreateSaturating(saisie); // 0
///
/// // Code générique
/// static T Somme&lt;T&gt;(IEnumerable&lt;T&gt; valeurs) where T : INumber&lt;T&gt;
/// {
///     T total = T.Zero;
///     foreach (T v in valeurs) total += v;
///     return total;
/// }
/// RNuint total = Somme(new RNuint[] { 1, 2, 3 }); // 6
/// </code>
/// </example>
public readonly record struct RNuint(nuint Value)
    : IRUnsignedInteger<RNuint>,
        IComparable
{
    #region Constantes

    /// <summary>Obtient la plus petite valeur possible d'un <see cref="RNuint"/> (0).</summary>
    public static RNuint MinValue => new(nuint.MinValue);

    /// <summary>Obtient la plus grande valeur possible d'un <see cref="RNuint"/> (<see cref="nuint.MaxValue"/>, soit <see cref="UIntPtr.MaxValue"/>).</summary>
    public static RNuint MaxValue => new(nuint.MaxValue);

    /// <summary>Obtient la valeur 0.</summary>
    public static RNuint Zero => new(0);

    /// <summary>Obtient la valeur 1.</summary>
    public static RNuint One => new(1);

    /// <summary>Obtient une valeur dont tous les bits sont à 1 (équivaut à <see cref="MaxValue"/>).</summary>
    public static RNuint AllBitsSet => new(nuint.MaxValue);

    /// <summary>Obtient la base de représentation du type (2, binaire).</summary>
    public static int Radix => 2;

    /// <summary>Élément neutre de l'addition (0).</summary>
    static RNuint IAdditiveIdentity<RNuint, RNuint>.AdditiveIdentity => Zero;

    /// <summary>Élément neutre de la multiplication (1).</summary>
    static RNuint IMultiplicativeIdentity<RNuint, RNuint>.MultiplicativeIdentity => One;

    #endregion

    #region Conversions

    /// <summary>Convertit implicitement un <see cref="nuint"/> en <see cref="RNuint"/>. Toujours valide.</summary>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>Un <see cref="RNuint"/> de même valeur.</returns>
    /// <remarks>Permet d'écrire <c>RNuint x = 5;</c> grâce à la conversion des constantes entières.</remarks>
    public static implicit operator RNuint(nuint value) => new(value);

    /// <summary>Convertit implicitement un <see cref="RNuint"/> en <see cref="nuint"/>. Toujours valide.</summary>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur brute.</returns>
    public static implicit operator nuint(RNuint value) => value.Value;

    /// <summary>
    /// Convertit explicitement un entier signé en <see cref="RNuint"/> en vérifiant ses bornes.
    /// Accepte aussi tout entier signé plus petit que <see cref="nint"/>.
    /// </summary>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>Un <see cref="RNuint"/> de même valeur.</returns>
    /// <exception cref="OverflowException">Si <paramref name="value"/> est négatif ou supérieur à <see cref="nuint.MaxValue"/>.</exception>
    public static explicit operator RNuint(nint value) => new(UnsignedArithmetic.FromSigned<nint, nuint>(value, nameof(RNuint)));

    #endregion

    #region Opérateurs arithmétiques

    /// <summary>Additionne deux valeurs.</summary>
    /// <param name="left">Premier terme.</param>
    /// <param name="right">Second terme.</param>
    /// <returns>La somme.</returns>
    /// <exception cref="OverflowException">Si la somme dépasse <see cref="nuint.MaxValue"/>.</exception>
    public static RNuint operator +(RNuint left, RNuint right) => new(UnsignedArithmetic.Add(left.Value, right.Value));

    /// <summary>Soustrait une valeur à une autre.</summary>
    /// <param name="left">Valeur de départ.</param>
    /// <param name="right">Valeur à soustraire.</param>
    /// <returns>La différence.</returns>
    /// <exception cref="OverflowException">Si <paramref name="right"/> est supérieur à <paramref name="left"/> (résultat négatif).</exception>
    public static RNuint operator -(RNuint left, RNuint right) => new(UnsignedArithmetic.Subtract(left.Value, right.Value, nameof(RNuint)));

    /// <summary>Multiplie deux valeurs.</summary>
    /// <param name="left">Premier facteur.</param>
    /// <param name="right">Second facteur.</param>
    /// <returns>Le produit.</returns>
    /// <exception cref="OverflowException">Si le produit dépasse <see cref="nuint.MaxValue"/>.</exception>
    public static RNuint operator *(RNuint left, RNuint right) => new(UnsignedArithmetic.Multiply(left.Value, right.Value));

    /// <summary>Effectue une division entière (troncature vers zéro).</summary>
    /// <param name="left">Dividende.</param>
    /// <param name="right">Diviseur.</param>
    /// <returns>Le quotient.</returns>
    /// <exception cref="DivideByZeroException">Si <paramref name="right"/> vaut 0.</exception>
    public static RNuint operator /(RNuint left, RNuint right) => new(left.Value / right.Value);

    /// <summary>Calcule le reste de la division entière.</summary>
    /// <param name="left">Dividende.</param>
    /// <param name="right">Diviseur.</param>
    /// <returns>Le reste.</returns>
    /// <exception cref="DivideByZeroException">Si <paramref name="right"/> vaut 0.</exception>
    public static RNuint operator %(RNuint left, RNuint right) => new(left.Value % right.Value);

    /// <summary>Incrémente la valeur de 1.</summary>
    /// <param name="value">Valeur à incrémenter.</param>
    /// <returns>La valeur augmentée de 1.</returns>
    /// <exception cref="OverflowException">Si <paramref name="value"/> vaut <see cref="MaxValue"/>.</exception>
    public static RNuint operator ++(RNuint value) => value + One;

    /// <summary>Décrémente la valeur de 1.</summary>
    /// <param name="value">Valeur à décrémenter.</param>
    /// <returns>La valeur diminuée de 1.</returns>
    /// <exception cref="OverflowException">Si <paramref name="value"/> vaut 0.</exception>
    public static RNuint operator --(RNuint value) => value - One;

    /// <summary>Plus unaire : retourne la valeur inchangée.</summary>
    /// <param name="value">Valeur d'entrée.</param>
    /// <returns><paramref name="value"/>.</returns>
    public static RNuint operator +(RNuint value) => value;

    /// <summary>Négation unaire. Seul 0 possède un opposé représentable.</summary>
    /// <param name="value">Valeur à négativer.</param>
    /// <returns>0 si <paramref name="value"/> vaut 0.</returns>
    /// <exception cref="OverflowException">Si <paramref name="value"/> est non nul.</exception>
    public static RNuint operator -(RNuint value) => Zero - value;

    #endregion

    #region Opérateurs binaires (sémantique nuint, aucun risque de négatif)

    /// <summary>ET bit à bit.</summary>
    /// <param name="left">Premier opérande.</param>
    /// <param name="right">Second opérande.</param>
    /// <returns>Le résultat du ET bit à bit.</returns>
    public static RNuint operator &(RNuint left, RNuint right) => new(left.Value & right.Value);

    /// <summary>OU bit à bit.</summary>
    /// <param name="left">Premier opérande.</param>
    /// <param name="right">Second opérande.</param>
    /// <returns>Le résultat du OU bit à bit.</returns>
    public static RNuint operator |(RNuint left, RNuint right) => new(left.Value | right.Value);

    /// <summary>OU exclusif bit à bit.</summary>
    /// <param name="left">Premier opérande.</param>
    /// <param name="right">Second opérande.</param>
    /// <returns>Le résultat du OU exclusif.</returns>
    public static RNuint operator ^(RNuint left, RNuint right) => new(left.Value ^ right.Value);

    /// <summary>Complément bit à bit (inverse tous les bits). <c>~0</c> donne <see cref="MaxValue"/>.</summary>
    /// <param name="value">Valeur à inverser.</param>
    /// <returns>Le complément.</returns>
    public static RNuint operator ~(RNuint value) => new(~value.Value);

    /// <summary>Décalage à gauche. Les bits sortants sont perdus sans exception, comme pour <see cref="nuint"/>.</summary>
    /// <param name="value">Valeur à décaler.</param>
    /// <param name="shiftAmount">Nombre de bits (seuls les 5 ou 6 bits de poids faible sont pris en compte).</param>
    /// <returns>La valeur décalée.</returns>
    public static RNuint operator <<(RNuint value, int shiftAmount) => new(value.Value << shiftAmount);

    /// <summary>Décalage à droite.</summary>
    /// <param name="value">Valeur à décaler.</param>
    /// <param name="shiftAmount">Nombre de bits (seuls les 5 ou 6 bits de poids faible sont pris en compte).</param>
    /// <returns>La valeur décalée.</returns>
    public static RNuint operator >>(RNuint value, int shiftAmount) => new(value.Value >> shiftAmount);

    /// <summary>Décalage à droite non signé (identique à <c>&gt;&gt;</c> pour un type non signé).</summary>
    /// <param name="value">Valeur à décaler.</param>
    /// <param name="shiftAmount">Nombre de bits (seuls les 5 ou 6 bits de poids faible sont pris en compte).</param>
    /// <returns>La valeur décalée.</returns>
    public static RNuint operator >>>(RNuint value, int shiftAmount) => new(value.Value >>> shiftAmount);

    #endregion

    #region Comparaisons

    /// <summary>Indique si <paramref name="left"/> est strictement inférieur à <paramref name="right"/>.</summary>
    /// <param name="left">Premier opérande.</param>
    /// <param name="right">Second opérande.</param>
    /// <returns><see langword="true"/> si <paramref name="left"/> &lt; <paramref name="right"/>.</returns>
    public static bool operator <(RNuint left, RNuint right) => left.Value < right.Value;

    /// <summary>Indique si <paramref name="left"/> est strictement supérieur à <paramref name="right"/>.</summary>
    /// <param name="left">Premier opérande.</param>
    /// <param name="right">Second opérande.</param>
    /// <returns><see langword="true"/> si <paramref name="left"/> &gt; <paramref name="right"/>.</returns>
    public static bool operator >(RNuint left, RNuint right) => left.Value > right.Value;

    /// <summary>Indique si <paramref name="left"/> est inférieur ou égal à <paramref name="right"/>.</summary>
    /// <param name="left">Premier opérande.</param>
    /// <param name="right">Second opérande.</param>
    /// <returns><see langword="true"/> si <paramref name="left"/> &lt;= <paramref name="right"/>.</returns>
    public static bool operator <=(RNuint left, RNuint right) => left.Value <= right.Value;

    /// <summary>Indique si <paramref name="left"/> est supérieur ou égal à <paramref name="right"/>.</summary>
    /// <param name="left">Premier opérande.</param>
    /// <param name="right">Second opérande.</param>
    /// <returns><see langword="true"/> si <paramref name="left"/> &gt;= <paramref name="right"/>.</returns>
    public static bool operator >=(RNuint left, RNuint right) => left.Value >= right.Value;

    /// <summary>Compare cette instance à un autre <see cref="RNuint"/>.</summary>
    /// <param name="other">Valeur à comparer.</param>
    /// <returns>Un nombre négatif si cette instance est plus petite, 0 si égale, positif si plus grande.</returns>
    public int CompareTo(RNuint other) => Value.CompareTo(other.Value);

    /// <summary>Compare cette instance à un objet.</summary>
    /// <param name="obj">Objet à comparer ; <see langword="null"/> est considéré comme plus petit que toute valeur.</param>
    /// <returns>Un nombre négatif si cette instance est plus petite, 0 si égale, positif si plus grande.</returns>
    /// <exception cref="ArgumentException">Si <paramref name="obj"/> n'est pas un <see cref="RNuint"/>.</exception>
    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        RNuint other => CompareTo(other),
        _ => throw new ArgumentException($"L'objet doit être de type {nameof(RNuint)}.", nameof(obj)),
    };

    #endregion

    #region INumber / INumberBase

    /// <summary>Valeur absolue. Un <see cref="RNuint"/> étant toujours positif, retourne la valeur inchangée.</summary>
    /// <param name="value">Valeur d'entrée.</param>
    /// <returns><paramref name="value"/>.</returns>
    public static RNuint Abs(RNuint value) => value;

    /// <summary>Restreint une valeur à l'intervalle [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="value">Valeur à restreindre.</param>
    /// <param name="min">Borne basse incluse.</param>
    /// <param name="max">Borne haute incluse.</param>
    /// <returns>La valeur ramenée dans l'intervalle.</returns>
    /// <exception cref="ArgumentException">Si <paramref name="min"/> est supérieur à <paramref name="max"/>.</exception>
    public static RNuint Clamp(RNuint value, RNuint min, RNuint max) => new(nuint.Clamp(value.Value, min.Value, max.Value));

    /// <summary>Copie le signe de <paramref name="sign"/> sur <paramref name="value"/>. Sans effet pour un type non signé.</summary>
    /// <param name="value">Valeur dont on garde la magnitude.</param>
    /// <param name="sign">Valeur dont on copie le signe (toujours positif).</param>
    /// <returns><paramref name="value"/>.</returns>
    public static RNuint CopySign(RNuint value, RNuint sign) => value;

    /// <summary>Retourne la plus grande des deux valeurs.</summary>
    /// <param name="x">Première valeur.</param>
    /// <param name="y">Seconde valeur.</param>
    /// <returns>Le maximum.</returns>
    public static RNuint Max(RNuint x, RNuint y) => x >= y ? x : y;

    /// <summary>Retourne la plus petite des deux valeurs.</summary>
    /// <param name="x">Première valeur.</param>
    /// <param name="y">Seconde valeur.</param>
    /// <returns>Le minimum.</returns>
    public static RNuint Min(RNuint x, RNuint y) => x <= y ? x : y;

    /// <summary>Équivalent de <see cref="Max"/> (pas de NaN pour un entier).</summary>
    /// <param name="x">Première valeur.</param>
    /// <param name="y">Seconde valeur.</param>
    /// <returns>Le maximum.</returns>
    public static RNuint MaxNumber(RNuint x, RNuint y) => Max(x, y);

    /// <summary>Équivalent de <see cref="Min"/> (pas de NaN pour un entier).</summary>
    /// <param name="x">Première valeur.</param>
    /// <param name="y">Seconde valeur.</param>
    /// <returns>Le minimum.</returns>
    public static RNuint MinNumber(RNuint x, RNuint y) => Min(x, y);

    /// <summary>Retourne la valeur de plus grande magnitude (équivaut à <see cref="Max"/> pour un type non signé).</summary>
    /// <param name="x">Première valeur.</param>
    /// <param name="y">Seconde valeur.</param>
    /// <returns>Le maximum.</returns>
    public static RNuint MaxMagnitude(RNuint x, RNuint y) => Max(x, y);

    /// <summary>Retourne la valeur de plus petite magnitude (équivaut à <see cref="Min"/> pour un type non signé).</summary>
    /// <param name="x">Première valeur.</param>
    /// <param name="y">Seconde valeur.</param>
    /// <returns>Le minimum.</returns>
    public static RNuint MinMagnitude(RNuint x, RNuint y) => Min(x, y);

    /// <summary>Équivalent de <see cref="MaxMagnitude"/> (pas de NaN pour un entier).</summary>
    /// <param name="x">Première valeur.</param>
    /// <param name="y">Seconde valeur.</param>
    /// <returns>Le maximum.</returns>
    public static RNuint MaxMagnitudeNumber(RNuint x, RNuint y) => Max(x, y);

    /// <summary>Équivalent de <see cref="MinMagnitude"/> (pas de NaN pour un entier).</summary>
    /// <param name="x">Première valeur.</param>
    /// <param name="y">Seconde valeur.</param>
    /// <returns>Le minimum.</returns>
    public static RNuint MinMagnitudeNumber(RNuint x, RNuint y) => Min(x, y);

    /// <summary>Retourne le signe de la valeur.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns>0 si <paramref name="value"/> vaut 0, sinon 1 (jamais -1).</returns>
    public static int Sign(RNuint value) => value.Value == default ? 0 : 1;

    /// <summary>Indique si la valeur est sous forme canonique. Toujours vrai pour un entier.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="true"/>.</returns>
    public static bool IsCanonical(RNuint value) => true;

    /// <summary>Indique si la valeur est un nombre complexe. Toujours faux.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="false"/>.</returns>
    public static bool IsComplexNumber(RNuint value) => false;

    /// <summary>Indique si la valeur est un entier pair.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="true"/> si la valeur est paire (0 compris).</returns>
    public static bool IsEvenInteger(RNuint value) => nuint.IsEvenInteger(value.Value);

    /// <summary>Indique si la valeur est un entier impair.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="true"/> si la valeur est impaire.</returns>
    public static bool IsOddInteger(RNuint value) => nuint.IsOddInteger(value.Value);

    /// <summary>Indique si la valeur est finie. Toujours vrai pour un entier.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="true"/>.</returns>
    public static bool IsFinite(RNuint value) => true;

    /// <summary>Indique si la valeur est un imaginaire pur. Toujours faux.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="false"/>.</returns>
    public static bool IsImaginaryNumber(RNuint value) => false;

    /// <summary>Indique si la valeur est infinie. Toujours faux.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="false"/>.</returns>
    public static bool IsInfinity(RNuint value) => false;

    /// <summary>Indique si la valeur est un entier. Toujours vrai.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="true"/>.</returns>
    public static bool IsInteger(RNuint value) => true;

    /// <summary>Indique si la valeur est NaN. Toujours faux.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="false"/>.</returns>
    public static bool IsNaN(RNuint value) => false;

    /// <summary>Indique si la valeur est négative. Toujours faux, c'est la garantie du type.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="false"/>.</returns>
    public static bool IsNegative(RNuint value) => false;

    /// <summary>Indique si la valeur est l'infini négatif. Toujours faux.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="false"/>.</returns>
    public static bool IsNegativeInfinity(RNuint value) => false;

    /// <summary>Indique si la valeur est « normale » au sens IEEE, c'est-à-dire non nulle pour un entier.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="true"/> si la valeur est non nulle.</returns>
    public static bool IsNormal(RNuint value) => value.Value != default;

    /// <summary>Indique si la valeur est positive (0 compris). Toujours vrai.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="true"/>.</returns>
    public static bool IsPositive(RNuint value) => true;

    /// <summary>Indique si la valeur est l'infini positif. Toujours faux.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="false"/>.</returns>
    public static bool IsPositiveInfinity(RNuint value) => false;

    /// <summary>Indique si la valeur est un nombre réel. Toujours vrai.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="true"/>.</returns>
    public static bool IsRealNumber(RNuint value) => true;

    /// <summary>Indique si la valeur est sous-normale. Toujours faux pour un entier.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="false"/>.</returns>
    public static bool IsSubnormal(RNuint value) => false;

    /// <summary>Indique si la valeur vaut 0.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="true"/> si la valeur vaut 0.</returns>
    public static bool IsZero(RNuint value) => value.Value == default;

    #endregion

    #region IBinaryNumber / IBinaryInteger

    /// <summary>Indique si la valeur est une puissance de 2.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns><see langword="true"/> si la valeur est une puissance de 2 (0 n'en est pas une).</returns>
    public static bool IsPow2(RNuint value) => nuint.IsPow2(value.Value);

    /// <summary>Calcule le logarithme entier en base 2 (position du bit de poids fort).</summary>
    /// <param name="value">Valeur d'entrée.</param>
    /// <returns>Le logarithme en base 2 arrondi vers le bas ; 0 pour une entrée nulle.</returns>
    public static RNuint Log2(RNuint value) => new(nuint.Log2(value.Value));

    /// <summary>Compte le nombre de bits à 1.</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns>Le nombre de bits à 1 (de 0 à 32 ou 64).</returns>
    public static RNuint PopCount(RNuint value) => new(nuint.PopCount(value.Value));

    /// <summary>Compte le nombre de bits à 0 en tête (poids fort).</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns>Le nombre de zéros de tête (32 ou 64 pour une valeur nulle).</returns>
    public static RNuint LeadingZeroCount(RNuint value) => new(nuint.LeadingZeroCount(value.Value));

    /// <summary>Compte le nombre de bits à 0 en queue (poids faible).</summary>
    /// <param name="value">Valeur à examiner.</param>
    /// <returns>Le nombre de zéros de queue (32 ou 64 pour une valeur nulle).</returns>
    public static RNuint TrailingZeroCount(RNuint value) => new(nuint.TrailingZeroCount(value.Value));

    /// <summary>Effectue une rotation des bits vers la gauche.</summary>
    /// <param name="value">Valeur à faire tourner.</param>
    /// <param name="rotateAmount">Nombre de bits de rotation.</param>
    /// <returns>La valeur après rotation.</returns>
    public static RNuint RotateLeft(RNuint value, int rotateAmount) => new(nuint.RotateLeft(value.Value, rotateAmount));

    /// <summary>Effectue une rotation des bits vers la droite.</summary>
    /// <param name="value">Valeur à faire tourner.</param>
    /// <param name="rotateAmount">Nombre de bits de rotation.</param>
    /// <returns>La valeur après rotation.</returns>
    public static RNuint RotateRight(RNuint value, int rotateAmount) => new(nuint.RotateRight(value.Value, rotateAmount));

    /// <summary>Calcule en une seule opération le quotient et le reste d'une division entière.</summary>
    /// <param name="left">Dividende.</param>
    /// <param name="right">Diviseur.</param>
    /// <returns>Un tuple (quotient, reste).</returns>
    /// <exception cref="DivideByZeroException">Si <paramref name="right"/> vaut 0.</exception>
    public static (RNuint Quotient, RNuint Remainder) DivRem(RNuint left, RNuint right)
    {
        (nuint quotient, nuint remainder) = nuint.DivRem(left.Value, right.Value);
        return (new RNuint(quotient), new RNuint(remainder));
    }

    /// <summary>Obtient le nombre d'octets écrits par <see cref="TryWriteBigEndian"/> et <see cref="TryWriteLittleEndian"/>.</summary>
    /// <returns>4 ou 8 selon la plateforme.</returns>
    public int GetByteCount() => IntPtr.Size;

    /// <summary>Obtient le nombre minimal de bits nécessaires pour représenter la valeur.</summary>
    /// <returns>Le nombre de bits significatifs (0 pour une valeur nulle).</returns>
    public int GetShortestBitLength() => (IntPtr.Size * 8) - (int)nuint.LeadingZeroCount(Value);

    /// <summary>Écrit la valeur en gros-boutiste (big-endian) dans un tampon.</summary>
    /// <param name="destination">Tampon de destination (4 ou 8 selon la plateforme octets minimum).</param>
    /// <param name="bytesWritten">Nombre d'octets écrits (4 ou 8 selon la plateforme en cas de succès, 0 sinon).</param>
    /// <returns><see langword="true"/> si le tampon était assez grand.</returns>
    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
        => TryWrite(BinaryPrimitives.TryWriteUIntPtrBigEndian(destination, Value), out bytesWritten);

    /// <summary>Écrit la valeur en petit-boutiste (little-endian) dans un tampon.</summary>
    /// <param name="destination">Tampon de destination (4 ou 8 selon la plateforme octets minimum).</param>
    /// <param name="bytesWritten">Nombre d'octets écrits (4 ou 8 selon la plateforme en cas de succès, 0 sinon).</param>
    /// <returns><see langword="true"/> si le tampon était assez grand.</returns>
    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
        => TryWrite(BinaryPrimitives.TryWriteUIntPtrLittleEndian(destination, Value), out bytesWritten);

    /// <summary>Lit une valeur encodée en gros-boutiste (big-endian).</summary>
    /// <param name="source">Octets à lire (longueur quelconque, les zéros de tête sont tolérés).</param>
    /// <param name="isUnsigned"><see langword="true"/> si les octets représentent un entier non signé ; <see langword="false"/> s'ils sont en complément à deux.</param>
    /// <returns>La valeur lue.</returns>
    /// <exception cref="OverflowException">Si la valeur encodée est négative ou dépasse <see cref="nuint.MaxValue"/>.</exception>
    public static RNuint ReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned)
        => TryReadBigEndian(source, isUnsigned, out RNuint value)
            ? value
            : throw new OverflowException($"Les octets ne représentent pas un {nameof(RNuint)} valide.");

    /// <summary>Lit une valeur encodée en petit-boutiste (little-endian).</summary>
    /// <param name="source">Octets à lire (longueur quelconque, les zéros de tête sont tolérés).</param>
    /// <param name="isUnsigned"><see langword="true"/> si les octets représentent un entier non signé ; <see langword="false"/> s'ils sont en complément à deux.</param>
    /// <returns>La valeur lue.</returns>
    /// <exception cref="OverflowException">Si la valeur encodée est négative ou dépasse <see cref="nuint.MaxValue"/>.</exception>
    public static RNuint ReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned)
        => TryReadLittleEndian(source, isUnsigned, out RNuint value)
            ? value
            : throw new OverflowException($"Les octets ne représentent pas un {nameof(RNuint)} valide.");

    /// <summary>Tente de lire une valeur encodée en gros-boutiste (big-endian).</summary>
    /// <param name="source">Octets à lire.</param>
    /// <param name="isUnsigned"><see langword="true"/> si les octets représentent un entier non signé ; <see langword="false"/> s'ils sont en complément à deux.</param>
    /// <param name="value">Valeur lue, ou 0 en cas d'échec.</param>
    /// <returns><see langword="false"/> si la valeur encodée est négative ou trop grande.</returns>
    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out RNuint value)
        => FromUInt(ReadBigEndianCore<nuint>(source, isUnsigned, out nuint raw), raw, out value);

    /// <summary>Tente de lire une valeur encodée en petit-boutiste (little-endian).</summary>
    /// <param name="source">Octets à lire.</param>
    /// <param name="isUnsigned"><see langword="true"/> si les octets représentent un entier non signé ; <see langword="false"/> s'ils sont en complément à deux.</param>
    /// <param name="value">Valeur lue, ou 0 en cas d'échec.</param>
    /// <returns><see langword="false"/> si la valeur encodée est négative ou trop grande.</returns>
    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out RNuint value)
        => FromUInt(ReadLittleEndianCore<nuint>(source, isUnsigned, out nuint raw), raw, out value);

    #endregion

    #region Conversions génériques (CreateChecked / CreateSaturating / CreateTruncating)

    // Implémentés explicitement pour retrouver l'API publique de nuint (RNuint.CreateChecked(...)) :
    // les membres par défaut d'interface ne sont pas visibles sur le type concret.
    // Les TryConvertFrom* délèguent à nuint, qui gère lui-même le repli vers TOther.TryConvertTo*.

    /// <summary>Crée un <see cref="RNuint"/> à partir de n'importe quel type numérique, en vérifiant les bornes.</summary>
    /// <typeparam name="TOther">Type numérique source (int, double, decimal, BigInteger…).</typeparam>
    /// <param name="value">Valeur à convertir. Les décimales sont tronquées vers zéro.</param>
    /// <returns>La valeur convertie.</returns>
    /// <exception cref="OverflowException">Si la valeur (après troncature) est négative ou dépasse <see cref="nuint.MaxValue"/>.</exception>
    /// <exception cref="NotSupportedException">Si aucune conversion n'existe entre les deux types.</exception>
    public static RNuint CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
        _ = TryConvertFromChecked(value, out RNuint result);
        return result;
    }

    /// <summary>Crée un <see cref="RNuint"/> à partir de n'importe quel type numérique, en écrêtant aux bornes.</summary>
    /// <typeparam name="TOther">Type numérique source.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>0 pour une source négative, <see cref="MaxValue"/> pour une source trop grande, sinon la valeur convertie.</returns>
    /// <exception cref="NotSupportedException">Si aucune conversion n'existe entre les deux types.</exception>
    public static RNuint CreateSaturating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
        _ = TryConvertFromSaturating(value, out RNuint result);
        return result;
    }

    /// <summary>
    /// Crée un <see cref="RNuint"/> à partir de n'importe quel type numérique, en ne gardant que les
    /// 32 ou 64 bits de poids faible. Contrairement à <see cref="nuint"/>, une source négative lève une exception.
    /// </summary>
    /// <typeparam name="TOther">Type numérique source.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <returns>La valeur tronquée.</returns>
    /// <exception cref="OverflowException">Si la source est négative (après troncature des décimales : -0.5 donne 0).</exception>
    /// <exception cref="NotSupportedException">Si aucune conversion n'existe entre les deux types.</exception>
    public static RNuint CreateTruncating<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
        _ = TryConvertFromTruncating(value, out RNuint result);
        return result;
    }

    /// <summary>Point d'extension du generic math utilisé par <see cref="CreateChecked{TOther}"/>.</summary>
    /// <typeparam name="TOther">Type numérique source.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <param name="result">Valeur convertie.</param>
    /// <returns><see langword="true"/> (les échecs se traduisent par une exception).</returns>
    /// <exception cref="OverflowException">Si la source est négative ou trop grande.</exception>
    /// <exception cref="NotSupportedException">Si aucune conversion n'existe entre les deux types.</exception>
    public static bool TryConvertFromChecked<TOther>(TOther value, out RNuint result)
        where TOther : INumberBase<TOther>
    {
        result = new RNuint(nuint.CreateChecked(value));
        return true;
    }

    /// <summary>Point d'extension du generic math utilisé par <see cref="CreateSaturating{TOther}"/>.</summary>
    /// <typeparam name="TOther">Type numérique source.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <param name="result">Valeur écrêtée entre 0 et <see cref="MaxValue"/>.</param>
    /// <returns><see langword="true"/>.</returns>
    /// <exception cref="NotSupportedException">Si aucune conversion n'existe entre les deux types.</exception>
    public static bool TryConvertFromSaturating<TOther>(TOther value, out RNuint result)
        where TOther : INumberBase<TOther>
    {
        result = new RNuint(nuint.CreateSaturating(value));
        return true;
    }

    /// <summary>Point d'extension du generic math utilisé par <see cref="CreateTruncating{TOther}"/>.</summary>
    /// <typeparam name="TOther">Type numérique source.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <param name="result">Valeur tronquée à 32 bits.</param>
    /// <returns><see langword="true"/> (les échecs se traduisent par une exception).</returns>
    /// <exception cref="OverflowException">Si la source est négative.</exception>
    /// <exception cref="NotSupportedException">Si aucune conversion n'existe entre les deux types.</exception>
    public static bool TryConvertFromTruncating<TOther>(TOther value, out RNuint result)
        where TOther : INumberBase<TOther>
    {
        // Conversion checked pour les négatifs : -0.5 donne 0, -1 lève une exception.
        result = new RNuint(TOther.IsNegative(value) ? nuint.CreateChecked(value) : nuint.CreateTruncating(value));
        return true;
    }

    /// <summary>Point d'extension du generic math permettant à <c>TOther.CreateChecked(rUint)</c> de fonctionner.</summary>
    /// <typeparam name="TOther">Type numérique cible.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <param name="result">Valeur convertie.</param>
    /// <returns><see langword="true"/>.</returns>
    /// <exception cref="OverflowException">Si la valeur ne tient pas dans <typeparamref name="TOther"/>.</exception>
    public static bool TryConvertToChecked<TOther>(RNuint value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        result = TOther.CreateChecked(value.Value);
        return true;
    }

    /// <summary>Point d'extension du generic math permettant à <c>TOther.CreateSaturating(rUint)</c> de fonctionner.</summary>
    /// <typeparam name="TOther">Type numérique cible.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <param name="result">Valeur écrêtée aux bornes de <typeparamref name="TOther"/>.</param>
    /// <returns><see langword="true"/>.</returns>
    public static bool TryConvertToSaturating<TOther>(RNuint value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        result = TOther.CreateSaturating(value.Value);
        return true;
    }

    /// <summary>Point d'extension du generic math permettant à <c>TOther.CreateTruncating(rUint)</c> de fonctionner.</summary>
    /// <typeparam name="TOther">Type numérique cible.</typeparam>
    /// <param name="value">Valeur à convertir.</param>
    /// <param name="result">Valeur tronquée à la taille de <typeparamref name="TOther"/>.</param>
    /// <returns><see langword="true"/>.</returns>
    public static bool TryConvertToTruncating<TOther>(RNuint value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        result = TOther.CreateTruncating(value.Value);
        return true;
    }

    #endregion

    #region Parsing (une chaîne négative lève une OverflowException, comme nuint)

    /// <summary>Analyse une chaîne avec la culture courante.</summary>
    /// <param name="s">Chaîne à analyser.</param>
    /// <returns>La valeur lue.</returns>
    /// <exception cref="ArgumentNullException">Si <paramref name="s"/> est <see langword="null"/>.</exception>
    /// <exception cref="FormatException">Si <paramref name="s"/> n'est pas un nombre valide.</exception>
    /// <exception cref="OverflowException">Si la valeur est négative ou trop grande.</exception>
    public static RNuint Parse(string s) => new(nuint.Parse(s, CultureInfo.CurrentCulture));

    /// <summary>Analyse une chaîne avec un fournisseur de format donné.</summary>
    /// <param name="s">Chaîne à analyser.</param>
    /// <param name="provider">Informations de format propres à la culture (<see langword="null"/> = culture courante).</param>
    /// <returns>La valeur lue.</returns>
    /// <exception cref="ArgumentNullException">Si <paramref name="s"/> est <see langword="null"/>.</exception>
    /// <exception cref="FormatException">Si <paramref name="s"/> n'est pas un nombre valide.</exception>
    /// <exception cref="OverflowException">Si la valeur est négative ou trop grande.</exception>
    public static RNuint Parse(string s, IFormatProvider? provider)
        => new(nuint.Parse(s, provider));

    /// <summary>Analyse une chaîne avec un style et un fournisseur de format donnés.</summary>
    /// <param name="s">Chaîne à analyser.</param>
    /// <param name="style">Styles autorisés (ex. <see cref="NumberStyles.HexNumber"/>).</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <returns>La valeur lue.</returns>
    /// <exception cref="ArgumentNullException">Si <paramref name="s"/> est <see langword="null"/>.</exception>
    /// <exception cref="FormatException">Si <paramref name="s"/> n'est pas un nombre valide.</exception>
    /// <exception cref="OverflowException">Si la valeur est négative ou trop grande.</exception>
    public static RNuint Parse(string s, NumberStyles style, IFormatProvider? provider)
        => new(nuint.Parse(s, style, provider));

    /// <summary>Analyse une plage de caractères avec un fournisseur de format donné.</summary>
    /// <param name="s">Caractères à analyser.</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <returns>La valeur lue.</returns>
    /// <exception cref="FormatException">Si <paramref name="s"/> n'est pas un nombre valide.</exception>
    /// <exception cref="OverflowException">Si la valeur est négative ou trop grande.</exception>
    public static RNuint Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => new(nuint.Parse(s, provider));

    /// <summary>Analyse une plage de caractères avec un style et un fournisseur de format donnés.</summary>
    /// <param name="s">Caractères à analyser.</param>
    /// <param name="style">Styles autorisés.</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <returns>La valeur lue.</returns>
    /// <exception cref="FormatException">Si <paramref name="s"/> n'est pas un nombre valide.</exception>
    /// <exception cref="OverflowException">Si la valeur est négative ou trop grande.</exception>
    public static RNuint Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
        => new(nuint.Parse(s, style, provider));

    /// <summary>Analyse un texte encodé en UTF-8.</summary>
    /// <param name="utf8Text">Octets UTF-8 à analyser.</param>
    /// <param name="style">Styles autorisés.</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <returns>La valeur lue.</returns>
    /// <exception cref="FormatException">Si le texte n'est pas un nombre valide.</exception>
    /// <exception cref="OverflowException">Si la valeur est négative ou trop grande.</exception>
    public static RNuint Parse(ReadOnlySpan<byte> utf8Text, NumberStyles style, IFormatProvider? provider)
        => new(nuint.Parse(utf8Text, style, provider));

    /// <summary>Tente d'analyser une chaîne avec la culture courante.</summary>
    /// <param name="s">Chaîne à analyser.</param>
    /// <param name="result">Valeur lue, ou 0 en cas d'échec.</param>
    /// <returns><see langword="false"/> si la chaîne est nulle, invalide, négative ou trop grande.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, out RNuint result)
        => FromUInt(nuint.TryParse(s, out nuint raw), raw, out result);

    /// <summary>Tente d'analyser une chaîne avec un fournisseur de format donné.</summary>
    /// <param name="s">Chaîne à analyser.</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <param name="result">Valeur lue, ou 0 en cas d'échec.</param>
    /// <returns><see langword="false"/> si la chaîne est nulle, invalide, négative ou trop grande.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out RNuint result)
        => FromUInt(nuint.TryParse(s, provider, out nuint raw), raw, out result);

    /// <summary>Tente d'analyser une chaîne avec un style et un fournisseur de format donnés.</summary>
    /// <param name="s">Chaîne à analyser.</param>
    /// <param name="style">Styles autorisés.</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <param name="result">Valeur lue, ou 0 en cas d'échec.</param>
    /// <returns><see langword="false"/> si la chaîne est nulle, invalide, négative ou trop grande.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out RNuint result)
        => FromUInt(nuint.TryParse(s, style, provider, out nuint raw), raw, out result);

    /// <summary>Tente d'analyser une plage de caractères avec un fournisseur de format donné.</summary>
    /// <param name="s">Caractères à analyser.</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <param name="result">Valeur lue, ou 0 en cas d'échec.</param>
    /// <returns><see langword="false"/> si le texte est invalide, négatif ou trop grand.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out RNuint result)
        => FromUInt(nuint.TryParse(s, provider, out nuint raw), raw, out result);

    /// <summary>Tente d'analyser une plage de caractères avec un style et un fournisseur de format donnés.</summary>
    /// <param name="s">Caractères à analyser.</param>
    /// <param name="style">Styles autorisés.</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <param name="result">Valeur lue, ou 0 en cas d'échec.</param>
    /// <returns><see langword="false"/> si le texte est invalide, négatif ou trop grand.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out RNuint result)
        => FromUInt(nuint.TryParse(s, style, provider, out nuint raw), raw, out result);

    /// <summary>Tente d'analyser un texte encodé en UTF-8.</summary>
    /// <param name="utf8Text">Octets UTF-8 à analyser.</param>
    /// <param name="style">Styles autorisés.</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <param name="result">Valeur lue, ou 0 en cas d'échec.</param>
    /// <returns><see langword="false"/> si le texte est invalide, négatif ou trop grand.</returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, NumberStyles style, IFormatProvider? provider, out RNuint result)
        => FromUInt(nuint.TryParse(utf8Text, style, provider, out nuint raw), raw, out result);

    #endregion

    #region Formatage

    /// <summary>Retourne la valeur sous forme de texte, avec la culture courante (ex. <c>"42"</c>).</summary>
    /// <returns>La représentation textuelle de la valeur.</returns>
    /// <remarks>Remplace l'affichage par défaut des records (<c>RNuint { Value = 42 }</c>).</remarks>
    public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

    /// <summary>Retourne la valeur sous forme de texte selon un format donné.</summary>
    /// <param name="format">Format numérique standard ou personnalisé (ex. <c>"D5"</c>, <c>"X8"</c>, <c>"N0"</c>).</param>
    /// <param name="formatProvider">Informations de format propres à la culture.</param>
    /// <returns>La représentation textuelle formatée.</returns>
    /// <exception cref="FormatException">Si <paramref name="format"/> n'est pas valide.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);

    /// <summary>Tente d'écrire la valeur formatée dans une plage de caractères, sans allocation.</summary>
    /// <param name="destination">Plage de destination.</param>
    /// <param name="charsWritten">Nombre de caractères écrits.</param>
    /// <param name="format">Format numérique.</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <returns><see langword="true"/> si la destination était assez grande.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    /// <summary>Tente d'écrire la valeur formatée en UTF-8, sans allocation.</summary>
    /// <param name="utf8Destination">Plage d'octets de destination.</param>
    /// <param name="bytesWritten">Nombre d'octets écrits.</param>
    /// <param name="format">Format numérique.</param>
    /// <param name="provider">Informations de format propres à la culture.</param>
    /// <returns><see langword="true"/> si la destination était assez grande.</returns>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => Value.TryFormat(utf8Destination, out bytesWritten, format, provider);

    #endregion

    #region Interne

    /// <summary>Adapte le résultat d'une méthode <c>Try*</c> de <see cref="nuint"/>.</summary>
    /// <param name="success">Succès de l'opération sur <see cref="nuint"/>.</param>
    /// <param name="raw">Valeur brute obtenue.</param>
    /// <param name="result">Valeur enveloppée, ou 0 en cas d'échec.</param>
    /// <returns><paramref name="success"/>.</returns>
    private static bool FromUInt(bool success, nuint raw, out RNuint result)
    {
        result = success ? new RNuint(raw) : default;
        return success;
    }

    /// <summary>Adapte le résultat d'une écriture binaire.</summary>
    /// <param name="success">Succès de l'écriture.</param>
    /// <param name="bytesWritten">4 en cas de succès, 0 sinon.</param>
    /// <returns><paramref name="success"/>.</returns>
    private static bool TryWrite(bool success, out int bytesWritten)
    {
        bytesWritten = success ? IntPtr.Size : 0;
        return success;
    }

    /// <summary>
    /// Accès générique à <c>TryReadBigEndian</c>, implémenté explicitement par <see cref="nuint"/>
    /// et donc inaccessible directement.
    /// </summary>
    private static bool ReadBigEndianCore<T>(ReadOnlySpan<byte> source, bool isUnsigned, out T value)
        where T : IBinaryInteger<T>
        => T.TryReadBigEndian(source, isUnsigned, out value);

    /// <summary>
    /// Accès générique à <c>TryReadLittleEndian</c>, implémenté explicitement par <see cref="nuint"/>
    /// et donc inaccessible directement.
    /// </summary>
    private static bool ReadLittleEndianCore<T>(ReadOnlySpan<byte> source, bool isUnsigned, out T value)
        where T : IBinaryInteger<T>
        => T.TryReadLittleEndian(source, isUnsigned, out value);

    #endregion
}
#endif

