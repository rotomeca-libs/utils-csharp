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
        public static void ClearTimeout(int number) => Internal.SetTimeout.Instance.ClearTimeOut(number);
    }
}
