using Rotomeca.Utils.Async.Helpers;

namespace Rotomeca.Utils.Async
{
    public static partial class Asynchronous
    {
        /// <summary>
        /// Exécute <paramref name="fn"/> de manière répétée jusqu'à ce qu'elle réussisse
        /// ou que le nombre maximal de tentatives soit atteint.
        /// </summary>
        /// <typeparam name="T">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction asynchrone à exécuter.</param>
        /// <param name="attempts">
        /// Nombre maximal de tentatives, garanti supérieur ou égal à 1
        /// via <see cref="AttemptNumber"/>.
        /// </param>
        /// <param name="delay">Délai en millisecondes entre chaque tentative.</param>
        /// <param name="cancellationToken">
        /// Token optionnel permettant d'interrompre les tentatives.
        /// </param>
        /// <returns>Le résultat de <paramref name="fn"/> lors du premier appel réussi.</returns>
        /// <remarks>
        /// Surcharge de convenance — convertit <paramref name="delay"/> en
        /// <see cref="TimeSpan"/> et délègue à
        /// <see cref="Retry{T}(Func{Task{T}}, AttemptNumber, TimeSpan, CancellationToken?)"/>.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Levée si <paramref name="cancellationToken"/> est annulé avant une tentative.
        /// </exception>
        /// <exception cref="AggregateException">
        /// Levée si toutes les tentatives échouent.
        /// </exception>
        /// <seealso cref="Retry{T}(Func{Task{T}}, AttemptNumber, TimeSpan, CancellationToken?)"/>
        public static Task<T> Retry<T>(
            Func<Task<T>> fn,
            AttemptNumber attempts,
            uint delay,
            CancellationToken? cancellationToken = null)
            => Retry(fn, attempts, TimeSpan.FromMilliseconds(delay), cancellationToken);

        /// <summary>
        /// Exécute <paramref name="fn"/> de manière répétée jusqu'à ce qu'elle réussisse
        /// ou que le nombre maximal de tentatives soit atteint.
        /// </summary>
        /// <typeparam name="T">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction asynchrone à exécuter.</param>
        /// <param name="attempts">
        /// Nombre maximal de tentatives, garanti supérieur ou égal à 1
        /// via <see cref="AttemptNumber"/>.
        /// </param>
        /// <param name="delay">Délai entre chaque tentative.</param>
        /// <param name="cancellationToken">
        /// Token optionnel permettant d'interrompre les tentatives.
        /// Vérifié avant chaque exécution de <paramref name="fn"/>.
        /// </param>
        /// <returns>Le résultat de <paramref name="fn"/> lors du premier appel réussi.</returns>
        /// <exception cref="OperationCanceledException">
        /// Levée si <paramref name="cancellationToken"/> est annulé avant une tentative.
        /// </exception>
        /// <exception cref="AggregateException">
        /// Levée si toutes les tentatives échouent.
        /// Contient la dernière exception levée par <paramref name="fn"/> comme inner exception.
        /// </exception>
        /// <example>
        /// <code>
        /// var result = await Asynchronous.Retry(
        ///     () => FetchDataAsync(),
        ///     attempts: 3,
        ///     delay: TimeSpan.FromMilliseconds(500));
        /// </code>
        /// </example>
        /// <seealso cref="Retry{T}(Func{Task{T}}, AttemptNumber, uint, CancellationToken?)"/>
        public static async Task<T> Retry<T>(
            Func<Task<T>> fn,
            AttemptNumber attempts,
            TimeSpan delay,
            CancellationToken? cancellationToken = null)
        {
            uint attemptCount = 0;

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
                    if (++attemptCount >= attempts)
                        throw new AggregateException(
                            $"Failed to execute the function after {attempts} attempts.", e);
                }

                await Sleep(delay, cancellationToken);
            }
        }
    }
}