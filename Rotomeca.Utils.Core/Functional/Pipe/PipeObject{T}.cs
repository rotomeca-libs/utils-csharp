using Rotomeca.Utils.Functional.Interfaces;
namespace Rotomeca.Utils.Functional
{
    /// <summary>
    /// Encapsule une valeur dans un pipeline fonctionnel permettant
    /// d'enchaîner des transformations de manière fluente.
    /// </summary>
    /// <typeparam name="T">Type de la valeur encapsulée.</typeparam>
    /// <remarks>
    /// <para>
    /// Cette classe est à usage interne — elle est retournée par
    /// <see cref="Pipeline.Start{T}"/> et par chaque appel à <see cref="Pipe{Y}"/>.
    /// Le consommateur interagit avec elle via ces deux points d'entrée.
    /// </para>
    /// <para>
    /// La conversion implicite vers <typeparamref name="T"/> permet de récupérer
    /// la valeur finale sans appel explicite à <see cref="Unpipe"/> :
    /// <code>
    /// string result = Pipeline.Start("hello").Pipe(s => s.ToUpper());
    /// // Équivalent à :
    /// string result = Pipeline.Start("hello").Pipe(s => s.ToUpper()).Unpipe();
    /// </code>
    /// </para>
    /// </remarks>
    /// <seealso cref="Pipeline"/>
    public sealed record PipeObject<T>(T Value): IPipeObject<T>
    {
        private readonly T _value = Value;

        /// <summary>
        /// Applique une fonction de transformation à la valeur courante
        /// et retourne un nouveau <see cref="PipeObject{Y}"/> contenant le résultat.
        /// </summary>
        /// <typeparam name="Y">Type de la valeur produite par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction de transformation à appliquer à la valeur courante.</param>
        /// <returns>
        /// Un nouveau <see cref="PipeObject{Y}"/> encapsulant le résultat de <paramref name="fn"/>,
        /// prêt pour une transformation supplémentaire.
        /// </returns>
        /// <example>
        /// <code>
        /// var result = Pipeline.Start(42)
        ///                      .Pipe(n => n * 2)   // PipeObject&lt;int&gt;(84)
        ///                      .Pipe(n => $"{n}"); // PipeObject&lt;string&gt;("84")
        /// </code>
        /// </example>
        public PipeObject<Y> Pipe<Y>(Func<T, Y> fn) => new(fn(_value));

        /// <summary>
        /// Termine le pipeline et retourne la valeur encapsulée.
        /// </summary>
        /// <returns>La valeur finale après toutes les transformations appliquées.</returns>
        /// <remarks>
        /// Équivalent à un cast implicite vers <typeparamref name="T"/>.
        /// Préférez la conversion implicite lorsque le contexte le permet,
        /// et réservez <see cref="Unpipe"/> aux cas où le type cible est ambigu.
        /// </remarks>
        /// <example>
        /// <code>
        /// int result = Pipeline.Start("hello world")
        ///                      .Pipe(s => s.Split(' '))
        ///                      .Pipe(words => words.Length)
        ///                      .Unpipe(); // 2
        /// </code>
        /// </example>
        public T Unpipe() => _value;

        /// <summary>
        /// Implémentation explicite de <see cref="IPipeObject{T}.Pipe{Y}"/>.
        /// Délègue à <see cref="Pipe{Y}"/> pour éviter la duplication de logique.
        /// </summary>
        IPipeObject<Y> IPipeObject<T>.Pipe<Y>(Func<T, Y> fn) => Pipe(fn);

        /// <summary>
        /// Convertit implicitement un <see cref="PipeObject{T}"/> en <typeparamref name="T"/>.
        /// </summary>
        /// <param name="pipeObject">L'objet pipeline à convertir.</param>
        /// <returns>La valeur encapsulée dans le pipeline.</returns>
        /// <remarks>
        /// Permet de terminer un pipeline sans appel explicite à <see cref="Unpipe"/> :
        /// <code>
        /// string result = Pipeline.Start("hello").Pipe(s => s.ToUpper());
        /// </code>
        /// </remarks>
        public static implicit operator T(PipeObject<T> pipeObject) => pipeObject._value;
    }
}