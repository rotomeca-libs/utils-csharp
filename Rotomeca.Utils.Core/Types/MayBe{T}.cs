namespace Rotomeca.Utils.Types
{
    /// <summary>
    /// Représente une valeur qui peut être présente ou absente,
    /// indépendamment de la nullabilité de <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="MayBe{T}"/> fonctionne uniformément pour les types valeur et les types référence,
    /// là où <see cref="Nullable{T}"/> est limité aux types valeur et où le simple <c>T?</c>
    /// ne distingue pas "valeur absente" de "valeur présente mais nulle".
    /// </para>
    /// <para>
    /// Équivalent C# du type <c>T | null</c> de TypeScript dans <c>@rotomeca/utils</c>.
    /// Le concept <c>undefined</c> de TypeScript n'a pas d'équivalent en C# et n'est pas modélisé.
    /// </para>
    /// <para>
    /// L'état vide est représenté par <see cref="Null"/> ou par <c>default(MayBe&lt;T&gt;)</c>.
    /// </para>
    /// <example>
    /// <code>
    /// MayBe&lt;string&gt; withValue = "hello";       // présent
    /// MayBe&lt;string&gt; withNull  = (string?)null; // présent, valeur nulle
    /// MayBe&lt;string&gt; empty     = MayBe&lt;string&gt;.Null; // absent
    ///
    /// if (withValue.HasValue)
    ///     Console.WriteLine(withValue.Value); // → hello
    ///
    /// string result = empty.GetValueOrDefault("fallback"); // → fallback
    /// </code>
    /// </example>
    /// </remarks>
    /// <typeparam name="T">Type de la valeur encapsulée.</typeparam>
    public readonly struct MayBe<T> : IEquatable<MayBe<T>>
    {
        private readonly T? _value;
        private readonly bool _hasValue;

        // ── Propriétés ───────────────────────────────────────────────────────

        /// <summary>
        /// Valeur encapsulée.
        /// </summary>
        /// <remarks>
        /// Retourne <see langword="default"/> si <see cref="HasValue"/> est <see langword="false"/>.
        /// Vérifier <see cref="HasValue"/> avant d'accéder à cette propriété.
        /// </remarks>
        public T? Value => _value;

        /// <summary>
        /// Indique si une valeur est présente, qu'elle soit nulle ou non.
        /// </summary>
        public bool HasValue => _hasValue;

        /// <summary>
        /// Indique si aucune valeur n'est présente.
        /// Équivalent de <c>!HasValue</c>.
        /// </summary>
        public bool IsEmpty => !_hasValue;

        // ── Constructeurs ────────────────────────────────────────────────────

        /// <summary>
        /// Crée une instance contenant <paramref name="value"/>.
        /// <see cref="HasValue"/> sera <see langword="true"/>, même si <paramref name="value"/>
        /// est <see langword="null"/>.
        /// </summary>
        /// <param name="value">Valeur à encapsuler, peut être <see langword="null"/>.</param>
        public MayBe(T? value)
        {
            _value = value;
            _hasValue = true;
        }

        /// <summary>
        /// Constructeur privé permettant de contrôler explicitement <see cref="HasValue"/>.
        /// Utilisé pour créer l'état vide.
        /// </summary>
        private MayBe(T? value, bool hasValue)
        {
            _value = value;
            _hasValue = hasValue;
        }

        // ── Factories ────────────────────────────────────────────────────────

        /// <summary>
        /// Instance vide représentant l'absence de valeur.
        /// Équivalent de <c>default(MayBe&lt;T&gt;)</c>.
        /// </summary>
        public static readonly MayBe<T> Null = default;

        /// <summary>
        /// Crée une instance contenant <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Valeur à encapsuler, peut être <see langword="null"/>.</param>
        /// <returns>Un <see cref="MayBe{T}"/> avec <see cref="HasValue"/> à <see langword="true"/>.</returns>
        public static MayBe<T> Some(T? value) => new(value, true);

        // ── Extraction ───────────────────────────────────────────────────────

        /// <summary>
        /// Retourne la valeur encapsulée si présente,
        /// sinon retourne <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="defaultValue">
        /// Valeur retournée si <see cref="HasValue"/> est <see langword="false"/>.
        /// Par défaut <see langword="default"/>.
        /// </param>
        /// <returns>
        /// La valeur encapsulée ou <paramref name="defaultValue"/>.
        /// </returns>
        public T? GetValueOrDefault(T? defaultValue = default)
            => _hasValue ? _value : defaultValue;

        /// <summary>
        /// Retourne la valeur encapsulée si présente,
        /// sinon invoque <paramref name="fallback"/> et retourne son résultat.
        /// </summary>
        /// <param name="fallback">
        /// Fonction invoquée uniquement si <see cref="HasValue"/> est <see langword="false"/>.
        /// </param>
        /// <returns>
        /// La valeur encapsulée ou le résultat de <paramref name="fallback"/>.
        /// </returns>
        public T? GetValueOrElse(Func<T?> fallback)
            => _hasValue ? _value : fallback();

        /// <summary>
        /// Déconstruit l'instance en ses deux composantes.
        /// </summary>
        /// <param name="hasValue">
        /// <see langword="true"/> si une valeur est présente.
        /// </param>
        /// <param name="value">
        /// La valeur encapsulée, ou <see langword="default"/> si absente.
        /// </param>
        /// <example>
        /// <code>
        /// var (hasValue, value) = mayBe;
        /// </code>
        /// </example>
        public void Deconstruct(out bool hasValue, out T? value)
        {
            hasValue = _hasValue;
            value = _value;
        }

        // ── Conversions implicites ────────────────────────────────────────────

        /// <summary>
        /// Convertit implicitement une valeur de type <typeparamref name="T"/>
        /// en <see cref="MayBe{T}"/> avec <see cref="HasValue"/> à <see langword="true"/>.
        /// </summary>
        /// <param name="value">Valeur à encapsuler.</param>
        public static implicit operator MayBe<T>(T? value) => new(value, true);

        /// <summary>
        /// Convertit implicitement un <see cref="MayBe{T}"/> en sa valeur encapsulée.
        /// </summary>
        /// <remarks>
        /// Retourne <see langword="default"/> si <see cref="HasValue"/> est <see langword="false"/>.
        /// </remarks>
        /// <param name="mayBe">Instance à convertir.</param>
        public static implicit operator T?(MayBe<T> mayBe) => mayBe._value;

        // ── Égalité ───────────────────────────────────────────────────────────

        /// <inheritdoc/>
        /// <remarks>
        /// Deux instances vides sont toujours égales.
        /// Deux instances présentes sont égales si leurs valeurs sont égales
        /// selon <see cref="EqualityComparer{T}.Default"/>.
        /// </remarks>
        public bool Equals(MayBe<T> other)
        {
            if (!_hasValue && !other._hasValue) return true;
            if (_hasValue != other._hasValue) return false;
            return EqualityComparer<T?>.Default.Equals(_value, other._value);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
            => obj is MayBe<T> other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() {
#if NETSTANDARD2_0
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _hasValue.GetHashCode();
                hash = hash * 31 + (_value?.GetHashCode() ?? 0);
                return hash;
            }
#else
    return _hasValue
        ? HashCode.Combine(true, _value)
        : HashCode.Combine(false);
#endif
        }

        /// <summary>
        /// Détermine si deux instances de <see cref="MayBe{T}"/> sont égales.
        /// </summary>
        public static bool operator ==(MayBe<T> left, MayBe<T> right) => left.Equals(right);

        /// <summary>
        /// Détermine si deux instances de <see cref="MayBe{T}"/> sont différentes.
        /// </summary>
        public static bool operator !=(MayBe<T> left, MayBe<T> right) => !left.Equals(right);

        // ── Affichage ─────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public override string ToString()
            => _hasValue ? _value?.ToString() ?? "null" : "null";
    }
}