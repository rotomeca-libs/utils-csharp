using Rotomeca.Utils.Types;
using static Rotomeca.Utils.Async.Asynchronous;

namespace Rotomeca.Utils.Functional
{
    /// <summary>
    /// Fournit des utilitaires de programmation fonctionnelle.
    /// </summary>
    /// <remarks>
    /// <para>
    /// La classe est <see langword="partial"/> — ses méthodes sont réparties
    /// sur plusieurs fichiers sources par domaine fonctionnel
    /// (<c>Function.Pipe.cs</c>, <c>Function.Memoize.cs</c>, etc.).
    /// </para>
    /// </remarks>
    public static partial class Function
    {
        /// <summary>
        /// Retarde l'exécution d'une action jusqu'à ce qu'un délai se soit écoulé
        /// sans nouvel appel. Chaque appel remet le timer à zéro.
        /// </summary>
        /// <param name="action">Action à exécuter après le délai.</param>
        /// <param name="ms">Délai en millisecondes avant l'exécution de <paramref name="action"/>.</param>
        /// <returns>
        /// Une nouvelle <see cref="Action"/> qui diffère l'appel à <paramref name="action"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Utile pour limiter les appels sur des événements haute fréquence
        /// comme la saisie dans un champ de recherche ou le redimensionnement d'une fenêtre.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Action onSearch = Asynchronous.Debounce(() => FetchResults(), 300);
        /// // Chaque frappe remet le timer à zéro — FetchResults n'est appelé
        /// // qu'après 300ms sans nouvelle frappe.
        /// </code>
        /// </example>
        /// <seealso cref="Throttle"/>
        public static Action Debounce(Action action, uint ms)
        {
            int timeoutId = 0;
            return () =>
            {
                ClearTimeout(timeoutId);
                timeoutId = SetTimeout(action, ms);
            };
        }

        /// <summary>
        /// Garantit qu'une action ne s'exécute pas plus d'une fois par tranche
        /// de temps donnée, quel que soit le nombre d'appels reçus.
        /// </summary>
        /// <param name="action">Action dont la fréquence d'exécution est à limiter.</param>
        /// <param name="ms">Intervalle minimal en millisecondes entre deux exécutions.</param>
        /// <returns>
        /// Une nouvelle <see cref="Action"/> dont la fréquence d'appel est limitée.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Contrairement à <see cref="Debounce"/>, le premier appel s'exécute immédiatement.
        /// Les appels suivants sont ignorés jusqu'à l'expiration de l'intervalle.
        /// </para>
        /// <para>
        /// Sur .NET 5 et supérieur, utilise <see cref="Nullable{T}"/> nativement.
        /// Sur <c>netstandard2.0</c> et <c>netstandard2.1</c>, utilise <see cref="MayBe{T}"/>
        /// en raison de comportements incompatibles/étranges avec <see cref="Nullable{T}"/> sur ces targets.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Action onScroll = Asynchronous.Throttle(() => UpdateScrollbar(), 100);
        /// // UpdateScrollbar ne s'exécute qu'une fois toutes les 100ms,
        /// // même si l'événement scroll se déclenche des centaines de fois.
        /// </code>
        /// </example>
        /// <seealso cref="Debounce"/>
        public static Action Throttle(Action action, uint ms)
        {
#if NET5_0_OR_GREATER
            long? lastCall = null;
#else
            MayBe<long> lastCall = MayBe<long>.Null;
#endif

            return () =>
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
#if NET5_0_OR_GREATER
                if (!lastCall.HasValue || now - lastCall.Value >= ms)
#else
                if (lastCall.IsEmpty || now - lastCall.Value >= ms)
#endif
                {
                    action();
                    lastCall = now;
                }
            };
        }

        /// <summary>
        /// Ne fait rien.
        /// </summary>
        /// <remarks>
        /// Utile comme callback vide, placeholder ou implémentation neutre
        /// là où une méthode est syntaxiquement requise.
        /// </remarks>
        /// <example>
        /// <code>
        /// Action callback = condition ? DoSomething : Function.Noop;
        /// callback();
        /// </code>
        /// </example>
        public static void Noop() { }

        /// <summary>
        /// Retourne la valeur reçue sans modification.
        /// </summary>
        /// <typeparam name="T">Type de la valeur.</typeparam>
        /// <param name="value">Valeur à retourner.</param>
        /// <returns>
        /// <paramref name="value"/> inchangé.
        /// </returns>
        /// <remarks>
        /// Utile comme transformateur neutre dans un pipeline ou comme
        /// valeur par défaut d'un paramètre de type <see cref="Func{T, TResult}"/>.
        /// </remarks>
        /// <example>
        /// <code>
        /// var result = Pipeline.Start(42)
        ///     .Pipe(Function.Identity) // aucune transformation
        ///     .Pipe(n => n * 2);       // 84
        /// </code>
        /// </example>
        public static T Identity<T>(T value) => value;
    }
}
