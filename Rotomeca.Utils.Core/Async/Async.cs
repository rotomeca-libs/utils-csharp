namespace Rotomeca.Utils.Async
{
    /// <summary>
    /// Fournit des utilitaires asynchrones inspirés des API JavaScript,
    /// portés pour un usage idiomatique en C#.
    /// </summary>
    public static partial class Tasks
    {
        /// <summary>
        /// Exécute toutes les fonctions en parallèle et retourne leurs résultats.
        /// </summary>
        /// <typeparam name="T">Type du résultat de chaque fonction.</typeparam>
        /// <param name="tasks">Fonctions asynchrones à exécuter simultanément.</param>
        /// <returns>
        /// Un tableau contenant les résultats dans le même ordre que <paramref name="tasks"/>,
        /// disponible quand toutes les fonctions ont terminé.
        /// </returns>
        /// <remarks>
        /// Toutes les tâches démarrent immédiatement et s'exécutent en parallèle.
        /// Si une tâche lève une exception, <see cref="Task.WhenAll(System.Threading.Tasks.Task[])"/> regroupe
        /// toutes les exceptions dans une <see cref="AggregateException"/>.
        /// </remarks>
        /// <example>
        /// <code>
        /// var results = await Async.Parallel(
        ///     () => FetchUserAsync(1),
        ///     () => FetchUserAsync(2),
        ///     () => FetchUserAsync(3)
        /// );
        /// </code>
        /// </example>
        /// <seealso cref="Sequential{T}(Func{Task{T}}[])"/>
        public static Task<T[]> Parallel<T>(params Func<Task<T>>[] tasks)
            => Task.WhenAll(tasks.Select(fn => fn()));

        /// <summary>
        /// Retourne un flux asynchrone exécutant chaque fonction séquentiellement,
        /// en émettant les résultats au fur et à mesure.
        /// </summary>
        /// <typeparam name="T">Type du résultat de chaque fonction.</typeparam>
        /// <param name="fns">Tableau de fonctions asynchrones à exécuter dans l'ordre.</param>
        /// <returns>
        /// Un <see cref="IAsyncEnumerable{T}"/> émettant chaque résultat
        /// dès qu'il est disponible.
        /// </returns>
        /// <remarks>
        /// Surcharge de convenance — délègue à
        /// <see cref="SequentialGenerator{T}(IEnumerable{Func{Task{T}}})"/>.
        /// </remarks>
        /// <seealso cref="SequentialGenerator{T}(IEnumerable{Func{Task{T}}})"/>
        public static IAsyncEnumerable<T> SequentialGenerator<T>(Func<Task<T>>[] fns)
            => SequentialGenerator(fns.AsEnumerable());


#if !NETSTANDARD2_0
        /// <summary>
        /// Retourne un flux asynchrone exécutant chaque fonction séquentiellement,
        /// en émettant les résultats au fur et à mesure.
        /// </summary>
        /// <typeparam name="T">Type du résultat de chaque fonction.</typeparam>
        /// <param name="fns">Fonctions asynchrones à exécuter dans l'ordre.</param>
        /// <returns>
        /// Un <see cref="IAsyncEnumerable{T}"/> émettant chaque résultat
        /// dès qu'il est disponible, sans attendre les suivants.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Contrairement à <see cref="Sequential{T}(IEnumerable{Func{Task{T}}})"/>
        /// qui attend toutes les tâches avant de retourner, ce générateur permet
        /// de traiter chaque résultat dès sa disponibilité via <c>await foreach</c>.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// await foreach (var result in Async.SequentialGenerator(fns))
        ///     Console.WriteLine(result);
        /// </code>
        /// </example>
        /// <seealso cref="Sequential{T}(IEnumerable{Func{Task{T}}})"/>
        public static async IAsyncEnumerable<T> SequentialGenerator<T>(
            IEnumerable<Func<Task<T>>> fns)
        {
            foreach (var fn in fns)
                yield return await fn();
        }
#else
       /// <summary>
        /// Retourne un flux asynchrone exécutant chaque fonction séquentiellement,
        /// en émettant les résultats au fur et à mesure.
        /// </summary>
        /// <typeparam name="T">Type du résultat de chaque fonction.</typeparam>
        /// <param name="fns">Fonctions asynchrones à exécuter dans l'ordre.</param>
        /// <returns>
        /// Un <see cref="IAsyncEnumerable{T}"/> émettant chaque résultat
        /// dès qu'il est disponible, sans attendre les suivants.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Contrairement à <see cref="Sequential{T}(IEnumerable{Func{Task{T}}})"/>
        /// qui attend toutes les tâches avant de retourner, ce générateur permet
        /// de traiter chaque résultat dès sa disponibilité via <c>await foreach</c>.
        /// </para>
        /// <para>
        /// Sur <c>netstandard2.0</c>, utilise <see cref="Internal.AsyncEnumerable{T}"/>
        /// — implémentation manuelle sans dépendance externe.
        /// Sur les autres cibles, le compilateur génère une state machine optimisée.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// await foreach (var result in Async.SequentialGenerator(fns))
        ///     Console.WriteLine(result);
        /// </code>
        /// </example>
        /// <seealso cref="Sequential{T}(IEnumerable{Func{Task{T}}})"/>
        public static IAsyncEnumerable<T> SequentialGenerator<T>(
            IEnumerable<Func<Task<T>>> fns)
            => new Internal.AsyncEnumerable<T>(fns);
#endif

        /// <summary>
        /// Exécute toutes les fonctions séquentiellement et retourne leurs résultats.
        /// </summary>
        /// <typeparam name="T">Type du résultat de chaque fonction.</typeparam>
        /// <param name="fns">Tableau de fonctions asynchrones à exécuter dans l'ordre.</param>
        /// <returns>
        /// Un tableau contenant les résultats dans le même ordre que <paramref name="fns"/>,
        /// disponible quand toutes les fonctions ont terminé.
        /// </returns>
        /// <remarks>
        /// Surcharge de convenance — délègue à
        /// <see cref="Sequential{T}(IEnumerable{Func{Task{T}}})"/>.
        /// </remarks>
        /// <seealso cref="Sequential{T}(IEnumerable{Func{Task{T}}})"/>
        public static Task<T[]> Sequential<T>(Func<Task<T>>[] fns)
            => Sequential(fns.AsEnumerable());

        /// <summary>
        /// Exécute toutes les fonctions séquentiellement et retourne leurs résultats.
        /// </summary>
        /// <typeparam name="T">Type du résultat de chaque fonction.</typeparam>
        /// <param name="fns">Fonctions asynchrones à exécuter dans l'ordre.</param>
        /// <returns>
        /// Un tableau contenant les résultats dans le même ordre que <paramref name="fns"/>,
        /// disponible quand toutes les fonctions ont terminé.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Chaque fonction est attendue avant d'exécuter la suivante — l'ordre
        /// d'exécution est garanti.
        /// </para>
        /// <para>
        /// Pour traiter les résultats au fur et à mesure plutôt qu'en bloc,
        /// préférez <see cref="SequentialGenerator{T}(IEnumerable{Func{Task{T}}})"/>
        /// avec <c>await foreach</c>.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var results = await Async.Sequential(
        ///     () => StepOneAsync(),
        ///     () => StepTwoAsync(),
        ///     () => StepThreeAsync()
        /// );
        /// </code>
        /// </example>
        /// <seealso cref="Parallel{T}(Func{Task{T}}[])"/>
        /// <seealso cref="SequentialGenerator{T}(IEnumerable{Func{Task{T}}})"/>
        public static async Task<T[]> Sequential<T>(IEnumerable<Func<Task<T>>> fns)
        {
            var results = new List<T>();

            foreach (var fn in fns)
                results.Add(await fn());

            return [.. results];
        }
    }
}