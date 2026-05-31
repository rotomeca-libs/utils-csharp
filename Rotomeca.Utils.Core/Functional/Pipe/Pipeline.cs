namespace Rotomeca.Utils.Functional
{
    /// <summary>
    /// Point d'entrée du pattern de pipeline fonctionnel.
    /// </summary>
    /// <remarks>
    /// Permet de construire un pipeline d'opérations de manière fluente
    /// en enchaînant des appels à <see cref="PipeObject{T}.Pipe{Y}"/>.
    /// <para>
    /// Syntaxe cross-langage (C# et TypeScript) :
    /// <code>
    /// Pipeline.Start("hello")
    ///         .Pipe(s => s.ToUpper())
    ///         .Pipe(s => s.Trim());
    /// </code>
    /// </para>
    /// <para>
    /// Pour une syntaxe plus idiomatique en C#, préférez l'extension
    /// <see cref="Rotomeca.Utils._____StPipeExtensions.Pipe{T, TResult}"/> :
    /// <code>
    /// "hello".Pipe(s => s.ToUpper())
    ///        .Pipe(s => s.Trim());
    /// </code>
    /// </para>
    /// </remarks>
    /// <seealso cref="PipeObject{T}"/>
    /// <seealso cref="Rotomeca.Utils._____StPipeExtensions"/>
    public static class Pipeline
    {
        /// <summary>
        /// Crée un nouveau pipeline à partir d'une valeur initiale.
        /// </summary>
        /// <typeparam name="T">Type de la valeur initiale.</typeparam>
        /// <param name="value">Valeur servant de point de départ au pipeline.</param>
        /// <returns>
        /// Un <see cref="PipeObject{T}"/> encapsulant <paramref name="value"/>
        /// et prêt à recevoir des transformations successives.
        /// </returns>
        /// <example>
        /// <code>
        /// string result = Pipeline.Start("  hello world  ")
        ///                         .Pipe(s => s.Trim())
        ///                         .Pipe(s => s.ToUpper());
        /// // result == "HELLO WORLD"
        /// </code>
        /// </example>
        public static PipeObject<T> Start<T>(T value) => new(value);
    }
}