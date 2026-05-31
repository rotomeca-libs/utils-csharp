namespace Rotomeca.Utils.Functional.Interfaces
{
    /// <summary>
    /// Définit le contrat d'un objet pipeline fonctionnel permettant
    /// d'enchaîner des transformations successives sur une valeur.
    /// </summary>
    /// <typeparam name="T">Type de la valeur encapsulée par le pipeline.</typeparam>
    /// <seealso cref="Rotomeca.Utils.Functional.PipeObject{T}"/>
    public interface IPipeObject<T>
    {
        /// <summary>
        /// Applique une fonction de transformation à la valeur courante
        /// et retourne un nouveau pipeline contenant le résultat.
        /// </summary>
        /// <typeparam name="Y">Type produit par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction de transformation à appliquer.</param>
        /// <returns>
        /// Un nouveau <see cref="IPipeObject{Y}"/> encapsulant le résultat,
        /// prêt pour une transformation supplémentaire.
        /// </returns>
        public IPipeObject<Y> Pipe<Y>(Func<T, Y> fn);

        /// <summary>
        /// Termine le pipeline et retourne la valeur finale.
        /// </summary>
        /// <returns>La valeur après toutes les transformations appliquées.</returns>
        public T Unpipe();
    }
}