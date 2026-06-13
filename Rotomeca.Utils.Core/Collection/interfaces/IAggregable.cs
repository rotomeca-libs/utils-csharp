using System;
using System.Collections.Generic;
using System.Text;

namespace Rotomeca.Utils.Collections.Interfaces
{
    /// <summary>
    /// Définit le contrat d'agrégation pour les types qui peuvent s'additionner entre eux.
    /// </summary>
    /// <remarks>
    /// Utilisé comme contrainte de type dans les surcharges pre-.NET 7 de
    /// <c>Arrays.Sum</c>, en remplacement de <c>INumber&lt;T&gt;</c>
    /// qui n'est pas disponible sur NETSTANDARD2.0.
    /// </remarks>
    /// <typeparam name="T">Le type qui implémente l'addition.</typeparam>
    public interface IAggregable<T>
    {
        /// <summary>
        /// Additionne la valeur courante avec <paramref name="b"/> et retourne le résultat.
        /// </summary>
        /// <param name="b">La valeur à additionner.</param>
        /// <returns>Le résultat de l'addition.</returns>
        T Add(T b);
    }
}