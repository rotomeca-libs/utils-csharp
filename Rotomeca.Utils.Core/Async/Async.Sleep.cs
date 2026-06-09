namespace Rotomeca.Utils.Async
{
    public static partial class Async
    {
        /// <summary>
        /// Suspend l'exécution asynchrone pendant le délai spécifié.
        /// </summary>
        /// <param name="ms">Durée de la suspension en millisecondes.</param>
        /// <returns>Une <see cref="Task"/> qui se termine après <paramref name="ms"/> millisecondes.</returns>
        /// <example>
        /// <code>
        /// await Async.Sleep(500); // attend 500ms
        /// </code>
        /// </example>
        /// <seealso cref="Sleep(uint, CancellationToken)"/>
        public static Task Sleep(uint ms) => Task.Delay(TimeSpan.FromMilliseconds(ms));

        /// <summary>
        /// Suspend l'exécution asynchrone pendant le délai spécifié,
        /// avec support d'annulation.
        /// </summary>
        /// <param name="ms">Durée de la suspension en millisecondes.</param>
        /// <param name="cancellationToken">Token permettant d'annuler la suspension avant son terme.</param>
        /// <returns>
        /// Une <see cref="Task"/> qui se termine après <paramref name="ms"/> millisecondes,
        /// ou immédiatement si <paramref name="cancellationToken"/> est annulé.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Levée si <paramref name="cancellationToken"/> est annulé avant l'expiration du délai.
        /// </exception>
        /// <example>
        /// <code>
        /// var cts = new CancellationTokenSource();
        /// cts.CancelAfter(200);
        /// await Async.Sleep(1000, cts.Token); // annulé après 200ms
        /// </code>
        /// </example>
        /// <seealso cref="Sleep(uint)"/>
        public static Task Sleep(uint ms, CancellationToken cancellationToken)
            => Task.Delay(TimeSpan.FromMilliseconds(ms), cancellationToken);

        /// <summary>
        /// Suspend l'exécution asynchrone pendant le délai spécifié sous forme de <see cref="TimeSpan"/>,
        /// avec support optionnel d'annulation.
        /// </summary>
        /// <param name="duration">Durée de la suspension.</param>
        /// <param name="cancellationToken">
        /// Token optionnel permettant d'annuler la suspension avant son terme.
        /// Si <see langword="null"/>, aucune annulation n'est possible.
        /// </param>
        /// <returns>
        /// Une <see cref="Task"/> qui se termine après <paramref name="duration"/>,
        /// ou immédiatement si <paramref name="cancellationToken"/> est annulé.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Levée si <paramref name="cancellationToken"/> est annulé avant l'expiration du délai.
        /// </exception>
        /// <example>
        /// <code>
        /// await Async.Sleep(TimeSpan.FromSeconds(2));
        ///
        /// var cts = new CancellationTokenSource();
        /// await Async.Sleep(TimeSpan.FromSeconds(2), cts.Token);
        /// </code>
        /// </example>
        /// <seealso cref="Sleep(uint)"/>
        public static Task Sleep(TimeSpan duration, CancellationToken? cancellationToken = null)
            => cancellationToken.HasValue
                ? Task.Delay(duration, cancellationToken.Value)
                : Task.Delay(duration);
    }
}
