namespace Rotomeca.Utils.Types.Interfaces
{
    /// <summary>
    /// Définit le contrat d'une valeur contrainte entre un minimum et un maximum.
    /// </summary>
    /// <typeparam name="T">
    /// Type de la valeur. Doit implémenter <see cref="IComparable{T}"/>
    /// pour permettre la comparaison ordinale.
    /// </typeparam>
    /// <seealso cref="Rotomeca.Utils.Types.ClampedValue{T}"/>
    public interface IClampedValue<T> where T : IComparable<T>
    {
        /// <summary>Borne inférieure — la valeur ne peut pas descendre en dessous.</summary>
        T Min { get; }

        /// <summary>Borne supérieure — la valeur ne peut pas dépasser.</summary>
        T Max { get; }

        /// <summary>Valeur courante, garantie dans l'intervalle [<see cref="Min"/>, <see cref="Max"/>].</summary>
        T Value { get; }

        /// <summary>
        /// Retourne une nouvelle instance avec <paramref name="value"/> clampée
        /// dans [<see cref="Min"/>, <see cref="Max"/>].
        /// </summary>
        /// <param name="value">Nouvelle valeur à appliquer.</param>
        /// <returns>
        /// Un <see cref="Rotomeca.Utils.Types.ClampedValue{T}"/> contenant
        /// <paramref name="value"/> clampée.
        /// </returns>
        ClampedValue<T> WithValue(T value);
    }
}