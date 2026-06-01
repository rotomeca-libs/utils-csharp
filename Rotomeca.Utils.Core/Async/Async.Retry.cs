namespace Rotomeca.Utils.Async
{
    public static partial class Async
    {
        /// <summary>
        /// Exécute <paramref name="fn"/> de manière répétée jusqu'à ce qu'elle réussisse
        /// ou que le nombre maximal de tentatives soit atteint.
        /// </summary>
        /// <typeparam name="T">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction asynchrone à exécuter.</param>
        /// <param name="attempts">Nombre maximal de tentatives. Doit être supérieur à 0.</param>
        /// <param name="delay">Délai en millisecondes entre chaque tentative.</param>
        /// <param name="cancellationToken">
        /// Token optionnel permettant d'interrompre les tentatives avant leur terme.
        /// </param>
        /// <returns>Le résultat de <paramref name="fn"/> lors du premier appel réussi.</returns>
        /// <remarks>
        /// Surcharge de convenance — convertit <paramref name="delay"/> en <see cref="TimeSpan"/>
        /// et délègue à <see cref="Retry{T}(Func{Task{T}}, uint, TimeSpan, CancellationToken?)"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Levée si <paramref name="attempts"/> est égal à 0.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Levée si <paramref name="cancellationToken"/> est annulé avant une tentative.
        /// </exception>
        /// <exception cref="AggregateException">
        /// Levée si toutes les tentatives échouent.
        /// Contient la dernière exception levée par <paramref name="fn"/> comme inner exception.
        /// </exception>
        /// <example>
        /// <code>
        /// var result = await Async.Retry(() => FetchDataAsync(), attempts: 3, delay: 500);
        /// </code>
        /// </example>
        /// <seealso cref="Retry{T}(Func{Task{T}}, uint, TimeSpan, CancellationToken?)"/>
        public static Task<T> Retry<T>(
            Func<Task<T>> fn,
            uint attempts,
            uint delay,
            CancellationToken? cancellationToken = null)
            => Retry(fn, attempts, TimeSpan.FromMilliseconds(delay), cancellationToken);

        /// <summary>
        /// Exécute <paramref name="fn"/> de manière répétée jusqu'à ce qu'elle réussisse
        /// ou que le nombre maximal de tentatives soit atteint.
        /// </summary>
        /// <typeparam name="T">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction asynchrone à exécuter.</param>
        /// <param name="attempts">Nombre maximal de tentatives. Doit être supérieur à 0.</param>
        /// <param name="delay">Délai entre chaque tentative.</param>
        /// <param name="cancellationToken">
        /// Token optionnel permettant d'interrompre les tentatives.
        /// Vérifié avant chaque exécution de <paramref name="fn"/>.
        /// </param>
        /// <returns>Le résultat de <paramref name="fn"/> lors du premier appel réussi.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Levée si <paramref name="attempts"/> est égal à 0.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Levée si <paramref name="cancellationToken"/> est annulé avant une tentative.
        /// </exception>
        /// <exception cref="AggregateException">
        /// Levée si toutes les tentatives échouent.
        /// Contient la dernière exception levée par <paramref name="fn"/> comme inner exception.
        /// </exception>
        /// <example>
        /// <code>
        /// // 3 tentatives avec 500ms entre chaque
        /// var result = await Async.Retry(() => FetchDataAsync(), 3, TimeSpan.FromMilliseconds(500));
        ///
        /// // Avec annulation
        /// var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        /// var result = await Async.Retry(() => FetchDataAsync(), 3, TimeSpan.FromSeconds(1), cts.Token);
        /// </code>
        /// </example>
        public static async Task<T> Retry<T>(
            Func<Task<T>> fn,
            uint attempts,
            TimeSpan delay,
            CancellationToken? cancellationToken = null)
        {
            if (attempts == 0) throw new ArgumentOutOfRangeException(nameof(attempts), "Must be > 0");

            uint @try = 0;

            while (true)
            {
                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                    throw new OperationCanceledException();

                try
                {
                    return await fn();
                }
                catch (Exception e)
                {
                    ++@try;

                    if (@try >= attempts)
                        throw new AggregateException(
                            $"Failed to execute the function after {attempts} attempts.", e);
                }

                await Sleep(delay, cancellationToken);
            }
        }
    }
}
