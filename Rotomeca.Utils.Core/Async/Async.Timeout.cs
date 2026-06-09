namespace Rotomeca.Utils.Async
{
    public static partial class Async
    {
        /// <summary>
        /// Planifie l'exécution d'une action après un délai donné, sans bloquer le thread courant.
        /// </summary>
        /// <param name="fn">Action à exécuter à l'expiration du délai.</param>
        /// <param name="delay">Délai en millisecondes avant l'exécution de <paramref name="fn"/>.</param>
        /// <returns>
        /// Identifiant unique du timeout, utilisable pour l'annuler via <see cref="ClearTimeout"/>.
        /// </returns>
        /// <remarks>
        /// Équivalent de <c>setTimeout(fn, delay)</c> en JavaScript.
        /// Le callback est exécuté sur un thread du pool — évitez d'y accéder
        /// à des ressources non thread-safe sans synchronisation.
        /// </remarks>
        /// <example>
        /// <code>
        /// int id = Async.SetTimeout(() => Console.WriteLine("Déclenché !"), 2000);
        /// // ...
        /// Async.ClearTimeout(id); // Annulation si nécessaire
        /// </code>
        /// </example>
        /// <seealso cref="ClearTimeout"/>
        public static int SetTimeout(Action fn, uint delay) => Internal.SetTimeout.Instance.Start(fn, delay);

        /// <summary>
        /// Annule un timeout planifié avant son déclenchement.
        /// </summary>
        /// <param name="number">Identifiant du timeout à annuler, retourné par <see cref="SetTimeout"/>.</param>
        /// <remarks>
        /// <para>
        /// Équivalent de <c>clearTimeout(id)</c> en JavaScript.
        /// </para>
        /// <para>
        /// Si l'identifiant est introuvable (timeout déjà déclenché ou invalide),
        /// la méthode ne fait rien et ne lève pas d'exception.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// int id = Async.SetTimeout(() => Console.WriteLine("Ne s'affichera pas"), 5000);
        /// Async.ClearTimeout(id);
        /// </code>
        /// </example>
        /// <seealso cref="SetTimeout"/>
        public static void ClearTimeout(int number) => Internal.SetTimeout.Instance.ClearTimeout(number);

        /// <summary>
        /// Exécute <paramref name="task"/> avec une limite de temps.
        /// Lance une exception si la tâche ne se termine pas dans le délai imparti.
        /// </summary>
        /// <typeparam name="T">Type du résultat retourné par <paramref name="task"/>.</typeparam>
        /// <param name="task">Tâche à surveiller.</param>
        /// <param name="timeout">Délai maximal accordé à <paramref name="task"/> pour se terminer.</param>
        /// <returns>Le résultat de <paramref name="task"/> si elle se termine avant <paramref name="timeout"/>.</returns>
        /// <exception cref="TimeoutException">
        /// Levée si <paramref name="task"/> ne se termine pas dans le délai imparti.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Implémenté via <see cref="System.Threading.Tasks.Task.WhenAny(System.Threading.Tasks.Task[])"/> — aucun thread n'est bloqué pendant l'attente.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Lance une TimeoutException si FetchDataAsync dépasse 3 secondes
        /// var result = await Async.Timeout(FetchDataAsync(), TimeSpan.FromSeconds(3));
        /// </code>
        /// </example>
        /// <seealso cref="Timeout{T}(Task{T}, uint)"/>
        public static async Task<T> Timeout<T>(Task<T> task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource();
            var delay = Task.Delay(timeout, cts.Token);

            if (await Task.WhenAny(task, delay) != task)
                throw new TimeoutException($"L'opération a dépassé le délai imparti.");

            cts.Cancel();
            return await task;
        }

        /// <summary>
        /// Exécute <paramref name="task"/> avec une limite de temps en millisecondes.
        /// Lance une exception si la tâche ne se termine pas dans le délai imparti.
        /// </summary>
        /// <typeparam name="T">Type du résultat retourné par <paramref name="task"/>.</typeparam>
        /// <param name="task">Tâche à surveiller.</param>
        /// <param name="ms">Délai maximal en millisecondes accordé à <paramref name="task"/>.</param>
        /// <returns>Le résultat de <paramref name="task"/> si elle se termine avant <paramref name="ms"/> millisecondes.</returns>
        /// <exception cref="TimeoutException">
        /// Levée si <paramref name="task"/> ne se termine pas dans le délai imparti.
        /// </exception>
        /// <remarks>
        /// Surcharge de convenance — convertit <paramref name="ms"/> en <see cref="TimeSpan"/>
        /// et délègue à <see cref="Timeout{T}(Task{T}, TimeSpan)"/>.
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = await Async.Timeout(FetchDataAsync(), 3000);
        /// </code>
        /// </example>
        /// <seealso cref="Timeout{T}(Task{T}, TimeSpan)"/>
        public static Task<T> Timeout<T>(Task<T> task, uint ms)
            => Timeout(task, TimeSpan.FromMilliseconds(ms));

    }
}
