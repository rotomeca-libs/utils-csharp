namespace Rotomeca.Utils.Types
{
    /// <summary>
    /// Valeur immuable contrainte dans un intervalle [<see cref="Min"/>, <see cref="Max"/>].
    /// </summary>
    /// <typeparam name="T">
    /// Type de la valeur. Doit implémenter <see cref="IComparable{T}"/>.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// Implémenté en <see langword="readonly struct"/> — aucune allocation heap,
    /// sémantique par valeur, pas de GC.
    /// </para>
    /// <para>
    /// Toute tentative d'assigner une valeur hors intervalle est silencieusement
    /// clampée au minimum ou au maximum, jamais levée comme exception.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var speed = new ClampedValue&lt;int&gt;(0, 100, 150);
    /// Console.WriteLine(speed.Value); // 100 — clampée au max
    ///
    /// var faster = speed.WithValue(80);
    /// Console.WriteLine(faster.Value); // 80
    ///
    /// int raw = speed; // conversion implicite → 100
    /// </code>
    /// </example>
    public readonly struct ClampedValue<T> : IEquatable<ClampedValue<T>>
        where T : IComparable<T>
    {
        /// <summary>Borne inférieure de l'intervalle.</summary>
        public T Min { get; }

        /// <summary>Borne supérieure de l'intervalle.</summary>
        public T Max { get; }

        /// <summary>
        /// Valeur courante, garantie dans [<see cref="Min"/>, <see cref="Max"/>].
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Crée une valeur contrainte dans [<paramref name="min"/>, <paramref name="max"/>].
        /// <paramref name="value"/> est clampée silencieusement si hors intervalle.
        /// </summary>
        /// <param name="min">Borne inférieure.</param>
        /// <param name="max">Borne supérieure.</param>
        /// <param name="value">Valeur initiale, clampée si nécessaire.</param>
        /// <exception cref="ArgumentException">
        /// Levée si <paramref name="min"/> est supérieur à <paramref name="max"/>.
        /// </exception>
        public ClampedValue(T min, T max, T value)
        {
            if (min.CompareTo(max) > 0)
                throw new ArgumentException($"min ({min}) doit être ≤ max ({max}).", nameof(min));

            Min = min;
            Max = max;
            Value = Clamp(value, min, max);
        }

        private static T Clamp(T value, T min, T max)
            => value.CompareTo(min) < 0 ? min
             : value.CompareTo(max) > 0 ? max
             : value;

        /// <summary>
        /// Retourne une nouvelle instance avec <paramref name="newValue"/> clampée
        /// dans le même intervalle [<see cref="Min"/>, <see cref="Max"/>].
        /// </summary>
        /// <param name="newValue">Nouvelle valeur à appliquer.</param>
        /// <returns>Nouvelle instance avec la valeur mise à jour.</returns>
        public ClampedValue<T> WithValue(T newValue) => new(Min, Max, newValue);

        /// <summary>
        /// Convertit implicitement en <typeparamref name="T"/>.
        /// Retourne <see cref="Value"/>.
        /// </summary>
        public static implicit operator T(ClampedValue<T> c) => c.Value;

        /// <summary>Détermine si deux instances sont égales.</summary>
        public static bool operator ==(ClampedValue<T> left, ClampedValue<T> right) => left.Equals(right);

        /// <summary>Détermine si deux instances sont différentes.</summary>
        public static bool operator !=(ClampedValue<T> left, ClampedValue<T> right) => !left.Equals(right);

        /// <inheritdoc/>
        /// <remarks>
        /// Deux instances sont égales si <see cref="Min"/>, <see cref="Max"/>
        /// et <see cref="Value"/> sont identiques.
        /// </remarks>
        public bool Equals(ClampedValue<T> other)
            => EqualityComparer<T>.Default.Equals(Value, other.Value)
            && EqualityComparer<T>.Default.Equals(Min, other.Min)
            && EqualityComparer<T>.Default.Equals(Max, other.Max);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is ClampedValue<T> other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
#if NETSTANDARD2_0
            unchecked
            {
                int h = 17;
                h = h * 31 + EqualityComparer<T>.Default.GetHashCode(Min!);
                h = h * 31 + EqualityComparer<T>.Default.GetHashCode(Max!);
                h = h * 31 + EqualityComparer<T>.Default.GetHashCode(Value!);
                return h;
            }
#else
            return HashCode.Combine(Min, Max, Value);
#endif
        }

        /// <summary>
        /// Déconstruit l'instance en ses trois composantes.
        /// </summary>
        /// <param name="min">Borne inférieure.</param>
        /// <param name="max">Borne supérieure.</param>
        /// <param name="value">Valeur courante.</param>
        /// <example>
        /// <code>
        /// var (min, max, value) = new ClampedValue&lt;int&gt;(0, 100, 50);
        /// </code>
        /// </example>
        public void Deconstruct(out T min, out T max, out T value)
        {
            min = Min;
            max = Max;
            value = Value;
        }

        /// <inheritdoc/>
        public override string ToString() => $"[{Min}, {Max}] = {Value}";
    }
}