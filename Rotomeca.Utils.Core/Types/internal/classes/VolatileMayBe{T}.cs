namespace Rotomeca.Utils.Types.Internal
{
    /// <summary>
    /// Conteneur immuable à usage interne permettant la publication thread-safe
    /// d'une valeur via <see cref="System.Threading.Volatile"/>.
    /// </summary>
    /// <typeparam name="T">Type de la valeur encapsulée.</typeparam>
    /// <remarks>
    /// <para>
    /// Contrairement à <see cref="Rotomeca.Core.Optionals.MayBe{T}"/> (struct),
    /// <see cref="VolatileMayBe{T}"/> est une classe — ce qui permet d'utiliser
    /// sa référence comme sentinelle volatile : <see langword="null"/> signifie
    /// "non initialisé", une référence non-nulle signifie "valeur prête".
    /// </para>
    /// <para>
    /// Le champ <see cref="Value"/> est <see langword="readonly"/> : une fois la référence
    /// visible par un autre thread via <see cref="System.Threading.Volatile.Read{T}"/>,
    /// la valeur est garantie correcte et complète (garantie de publication sûre du CLR).
    /// </para>
    /// <para>
    /// Utilisé exclusivement par le pattern double-checked locking de
    /// <see cref="Rotomeca.Utils.Functional.Function"/>.
    /// </para>
    /// </remarks>
    internal sealed class VolatileMayBe<T>
    {
        /// <summary>
        /// Valeur encapsulée. Immuable — définie une seule fois dans le constructeur.
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// Crée une instance contenant <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Valeur à encapsuler.</param>
        internal VolatileMayBe(T value) => Value = value;

        /// <summary>
        /// Convertit implicitement une valeur de type <typeparamref name="T"/>
        /// en <see cref="VolatileMayBe{T}"/>.
        /// </summary>
        /// <param name="value">Valeur à encapsuler.</param>
        public static implicit operator VolatileMayBe<T>(T value) => new(value);

        /// <summary>
        /// Convertit implicitement un <see cref="VolatileMayBe{T}"/> en sa valeur encapsulée.
        /// </summary>
        /// <param name="mayBe">Instance à convertir.</param>
        public static implicit operator T(VolatileMayBe<T> mayBe) => mayBe.Value;
    }
}

namespace Rotomeca.Utils
{
    /// <summary>
    /// Extensions internes sur <see cref="Types.Internal.VolatileMayBe{T}"/> nullable,
    /// permettant d'interroger l'état d'initialisation sans déréférencer directement
    /// une référence potentiellement nulle.
    /// </summary>
    internal static class _____StVolatileMayBe
    {
        /// <summary>
        /// Indique si la valeur a été initialisée.
        /// </summary>
        /// <typeparam name="T">Type de la valeur encapsulée.</typeparam>
        /// <param name="mayBe">Instance à tester, peut être <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> si <paramref name="mayBe"/> est non-<see langword="null"/> ;
        /// <see langword="false"/> sinon.
        /// </returns>
        public static bool HasValue<T>(this Types.Internal.VolatileMayBe<T>? mayBe)
            => mayBe is not null;

        /// <summary>
        /// Indique si la valeur n'a pas encore été initialisée.
        /// Équivalent de <c>!HasValue</c>.
        /// </summary>
        /// <typeparam name="T">Type de la valeur encapsulée.</typeparam>
        /// <param name="mayBe">Instance à tester, peut être <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> si <paramref name="mayBe"/> est <see langword="null"/> ;
        /// <see langword="false"/> sinon.
        /// </returns>
        public static bool IsEmpty<T>(this Types.Internal.VolatileMayBe<T>? mayBe)
            => mayBe is null;
    }
}