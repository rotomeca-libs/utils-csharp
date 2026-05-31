using Rotomeca.Utils.Functional;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rotomeca.Utils
{
    /// <summary>
    /// Méthodes d'extension permettant d'utiliser le pipeline fonctionnel
    /// directement sur n'importe quelle valeur.
    /// </summary>
    /// <remarks>
    /// Ces extensions offrent une syntaxe idiomatique C# en complément
    /// de <see cref="Pipeline.Start{T}"/>, qui est la syntaxe cross-langage
    /// partagée avec TypeScript et PHP.
    /// </remarks>
    /// <seealso cref="Pipeline"/>
    /// <seealso cref="PipeObject{T}"/>
    public static class _____StPipeExtensions
    {
        /// <summary>
        /// Applique une fonction de transformation à la valeur courante
        /// et retourne le résultat encapsulé dans un <see cref="PipeObject{TResult}"/>.
        /// </summary>
        /// <typeparam name="T">Type de la valeur source.</typeparam>
        /// <typeparam name="TResult">Type du résultat produit par <paramref name="fn"/>.</typeparam>
        /// <param name="value">Valeur sur laquelle la transformation est appliquée.</param>
        /// <param name="fn">Fonction de transformation à appliquer.</param>
        /// <returns>
        /// Un <see cref="PipeObject{TResult}"/> encapsulant le résultat,
        /// permettant de chaîner d'autres transformations.
        /// </returns>
        /// <example>
        /// <code>
        /// // Syntaxe idiomatique C#
        /// string result = "  hello world  "
        ///     .Pipe(s => s.Trim())
        ///     .Pipe(s => s.ToUpper());
        /// // result == "HELLO WORLD"
        /// </code>
        /// </example>
        /// <seealso cref="Pipeline.Start{T}"/>
        public static PipeObject<TResult> Pipe<T, TResult>(this T value, Func<T, TResult> fn) => Pipeline.Start(fn(value));
    }
}