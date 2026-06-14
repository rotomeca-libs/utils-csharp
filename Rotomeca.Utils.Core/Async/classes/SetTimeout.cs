using System.Collections.Concurrent;

namespace Rotomeca.Utils.Async.Internal
{
    /// <summary>
    /// Gestionnaire interne des timeouts, inspiré du <c>setTimeout</c> JavaScript.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implémenté en singleton — une seule instance gère l'ensemble des timers actifs.
    /// Chaque timeout est identifié par un <see cref="uint"/> incrémental unique,
    /// à la manière des identifiants retournés par <c>setTimeout</c> en JavaScript.
    /// </para>
    /// <para>
    /// Les timers sont stockés dans un <see cref="ConcurrentDictionary{TKey,TValue}"/>
    /// pour garantir la sûreté des accès concurrents, les callbacks étant exécutés
    /// sur des threads du pool.
    /// </para>
    /// <para>
    /// Cette classe est à usage interne uniquement. Utilisez <see cref="Rotomeca.Utils.Async.Asynchronous"/>
    /// pour accéder aux fonctionnalités exposées publiquement.
    /// </para>
    /// </remarks>
    /// <seealso cref="Rotomeca.Utils.Async.Asynchronous"/>
    internal sealed class SetTimeout: IDisposable
    {
        /// <summary>
        /// Dictionnaire thread-safe associant chaque identifiant de timeout à son timer.
        /// </summary>
        private readonly ConcurrentDictionary<int, System.Timers.Timer> _timeOuts = new();

        /// <summary>
        /// Compteur utilisé pour générer les identifiants uniques des timeouts.
        /// </summary>
        /// <remarks>
        /// Déclaré en <see cref="int"/> plutôt qu'en <see cref="uint"/> pour permettre
        /// l'utilisation de <see cref="Interlocked.Increment(ref int)"/>, qui exige
        /// une référence directe au champ partagé pour garantir l'atomicité.
        /// La valeur est castée en <see cref="uint"/> à la sortie — elle reste toujours positive.
        /// </remarks>
        private int _nextId = 0;

        /// <summary>
        /// Instance unique du gestionnaire de timeouts (pattern Singleton).
        /// </summary>
        public static SetTimeout Instance { get; } = new SetTimeout();

        /// <summary>
        /// Constructeur privé — instanciation réservée au singleton <see cref="Instance"/>.
        /// </summary>
        private SetTimeout() { }

        /// <summary>
        /// Planifie l'exécution d'une action après un délai donné.
        /// </summary>
        /// <param name="fn">Action à exécuter à l'expiration du délai.</param>
        /// <param name="delay">Délai en millisecondes avant l'exécution de <paramref name="fn"/>.</param>
        /// <returns>
        /// Identifiant unique du timeout, utilisable pour l'annuler via
        /// <see cref="ClearTimeout"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Le timer est configuré en <c>AutoReset = false</c> : il ne se déclenche
        /// qu'une seule fois, puis se détruit automatiquement.
        /// </para>
        /// <para>
        /// L'identifiant est généré via <see cref="Interlocked.Increment(ref int)"/>
        /// directement sur le champ <see cref="_nextId"/>, garantissant l'atomicité
        /// en environnement multi-thread.
        /// </para>
        /// <para>
        /// Le timer est enregistré dans <see cref="_timeOuts"/> avant son démarrage
        /// afin d'éviter toute race condition entre le déclenchement et l'enregistrement.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// int id = Asynchronous.SetTimeout(() => Console.WriteLine("Timeout !"), 1000);
        /// </code>
        /// </example>
        public int Start(Action fn, uint delay)
        {
            // Incrément atomique directement sur le champ partagé
            var id = Interlocked.Increment(ref _nextId);

            var timer = new System.Timers.Timer(delay)
            {
                AutoReset = false // Déclenche une seule fois
            };

            timer.Elapsed += (sender, e) =>
            {
                // Nettoyage du dictionnaire avant l'exécution du callback
                if (_timeOuts.TryRemove(id, out var t))
                    t.Dispose();

                fn();
            };

            // Enregistrement avant démarrage pour éviter toute race condition
            _timeOuts[id] = timer;
            timer.Start();

            return id;
        }

        /// <summary>
        /// Annule un timeout planifié avant son déclenchement.
        /// </summary>
        /// <param name="number">Identifiant du timeout à annuler, retourné par <see cref="Start"/>.</param>
        /// <remarks>
        /// Si l'identifiant est introuvable (timeout déjà déclenché ou invalide),
        /// la méthode ne fait rien et ne lève pas d'exception.
        /// </remarks>
        /// <example>
        /// <code>
        /// uint id = Asynchronous.SetTimeout(() => Console.WriteLine("Ne s'affichera pas"), 5000);
        /// Asynchronous.ClearTimeout(id); // Annulation avant déclenchement
        /// </code>
        /// </example>
        public void ClearTimeout(int number)
        {
            if (_timeOuts.TryRemove(number, out var timer))
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var timer in _timeOuts)
            {
                timer.Value.Stop();
                timer.Value.Dispose();
            }

            _timeOuts.Clear();
        }
    }
}