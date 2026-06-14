using Rotomeca.Core.Optionals;
using System.Collections.Concurrent;

namespace Rotomeca.Utils.Functional
{
    public static partial class Function
    {
        // ─────────────────────────────────────────────────────────────────────
        // Helpers privés — noyaux partagés du cache
        // ─────────────────────────────────────────────────────────────────────

#if NET5_0_OR_GREATER
        /// <summary>
        /// Noyau du cache pour les surcharges à clé non nulle (NET5+).
        /// </summary>
        /// <remarks>
        /// Utilisé par les surcharges 2-arg et 3-arg dont les clés sont des tuples struct
        /// (toujours non nuls). La surcharge 1-arg gère le cas <see langword="null"/>
        /// séparément et ne délègue pas ici.
        /// </remarks>
        private static Func<TKey, TResult> MemoizeCore<TKey, TResult>(Func<TKey, TResult> fn)
            where TKey : notnull
        {
            ConcurrentDictionary<TKey, TResult>? cache = null;
            return key =>
            {
                if ((cache ??= []).TryGetValue(key, out TResult? value)) return value!;
                var result = fn(key);
                cache![key] = result;
                return result;
            };
        }
#else
        /// <summary>
        /// Récupère ou calcule-et-insère une valeur dans le cache (netstandard2.x).
        /// L'initialisation de <paramref name="cache"/> est thread-safe via <see cref="Lazy{T}"/>.
        /// </summary>
        /// <typeparam name="TKey">Type de la clé (struct MayBe ou tuple de MayBe — toujours notnull).</typeparam>
        /// <typeparam name="TResult">Type du résultat.</typeparam>
        /// <param name="cache">Cache partagé, initialisé au premier appel.</param>
        /// <param name="key">Clé calculée par l'appelant (déjà wrappée dans MayBe si nécessaire).</param>
        /// <param name="valueFactory">Fabrique du résultat si la clé est absente du cache.</param>
        private static TResult GetOrAddLegacy<TKey, TResult>(
            Lazy<ConcurrentDictionary<TKey, TResult>> cache,
            TKey key,
            Func<TResult> valueFactory)
            where TKey : notnull
        {
            var dict = cache.Value;
            if (dict.TryGetValue(key, out TResult value)) return value;
            value = valueFactory();
            dict[key] = value;
            return value;
        }
#endif

        // ─────────────────────────────────────────────────────────────────────
        // API publique
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne une version mémoïsée de <paramref name="fn"/> sans argument,
        /// garantissant que la fonction n'est exécutée qu'une seule fois.
        /// </summary>
        /// <typeparam name="TResult">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction pure sans argument à mémoïser.</param>
        /// <returns>
        /// Une nouvelle fonction qui retourne le résultat mis en cache dès le deuxième appel,
        /// sans réexécuter <paramref name="fn"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Contrairement aux surcharges avec arguments, aucun dictionnaire n'est nécessaire —
        /// le résultat est unique et stocké directement dans un <see cref="MayBe{T}"/>.
        /// C'est essentiellement un pattern de <b>lazy singleton</b>.
        /// </para>
        /// <para>
        /// Ne pas utiliser avec des fonctions impures (<c>DateTime.Now</c>, <c>Random</c>,
        /// effets de bord) : le premier résultat sera retourné indéfiniment.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var getConfig = Function.Memoize(() => LoadConfigFromDisk());
        /// getConfig(); // chargement effectué, résultat mis en cache
        /// getConfig(); // retourné depuis le cache, LoadConfigFromDisk() n'est plus appelé
        /// </code>
        /// </example>
        public static Func<TResult> Memoize<TResult>(Func<TResult> fn)
        {
            MayBe<TResult> result = MayBe<TResult>.Null;
            return () =>
            {
                if (result.IsEmpty) result = fn();
                return result.Value!;
            };
        }

        /// <summary>
        /// Retourne une version mémoïsée de <paramref name="fn"/> qui met en cache
        /// ses résultats selon l'argument reçu.
        /// </summary>
        /// <typeparam name="TArg1">Type de l'argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TResult">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction pure à mémoïser.</param>
        /// <returns>
        /// Une nouvelle fonction qui retourne le résultat depuis le cache si l'argument
        /// a déjà été rencontré, ou invoque <paramref name="fn"/> et met le résultat en cache sinon.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Le cache est thread-safe — implémenté via <see cref="ConcurrentDictionary{TKey,TValue}"/>.
        /// </para>
        /// <para>
        /// Les arguments <see langword="null"/> sont supportés et mis en cache séparément
        /// du dictionnaire principal.
        /// </para>
        /// <para>
        /// Ne pas utiliser avec des fonctions impures (<c>DateTime.Now</c>, <c>Random</c>,
        /// effets de bord) : le cache retournerait indéfiniment le premier résultat obtenu.
        /// </para>
        /// <para>
        /// Le cache est illimité — ne pas utiliser avec un nombre élevé de valeurs d'entrées
        /// distinctes sous peine de pression mémoire.
        /// </para>
        /// <para>
        /// Sur .NET 5 et supérieur, l'argument <see langword="null"/> est géré via un tableau
        /// dédié d'un élément, et les clés non nulles sont stockées directement dans le
        /// <see cref="ConcurrentDictionary{TKey,TValue}"/> sans surcoût de wrapping.
        /// Sur <c>netstandard2.0</c> et <c>netstandard2.1</c>, toutes les clés — y compris
        /// <see langword="null"/> — sont encapsulées dans <see cref="MayBe{T}"/> via
        /// <c>GetOrAddLegacy</c>.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var expensiveCalc = Function.Memoize((int n) => Fibonacci(n));
        /// expensiveCalc(40); // calcul effectué, résultat mis en cache
        /// expensiveCalc(40); // retourné depuis le cache
        /// expensiveCalc(41); // nouveau calcul
        /// </code>
        /// </example>
#if NET5_0_OR_GREATER
        public static Func<TArg1, TResult> Memoize<TArg1, TResult>(Func<TArg1, TResult> fn)
        {
            // Cas null isolé : un seul résultat possible, clé incompatible avec notnull
            TResult[]? argNullResult = null;
#pragma warning disable CS8714 // TArg1 peut être nullable — null intercepté avant le dict
            ConcurrentDictionary<TArg1, TResult>? cache = null;
#pragma warning restore CS8714

            return arg =>
            {
                if (arg is null) return (argNullResult ??= [fn(arg)])[0];
                if ((cache ??= []).TryGetValue(arg, out TResult? value)) return value!;
                var result = fn(arg);
                cache![arg] = result;
                return result;
            };
        }
#else
        public static Func<TArg1, TResult> Memoize<TArg1, TResult>(Func<TArg1, TResult> fn)
        {
            var cache = new Lazy<ConcurrentDictionary<MayBe<TArg1>, TResult>>(() => []);
            return arg => GetOrAddLegacy(cache, (MayBe<TArg1>)arg, () => fn(arg));
        }
#endif

        /// <summary>
        /// Retourne une version mémoïsée de <paramref name="fn"/> qui met en cache
        /// ses résultats selon les 2 arguments reçus.
        /// </summary>
        /// <typeparam name="TArg1">Type du premier argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TArg2">Type du deuxième argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TResult">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction pure à mémoïser.</param>
        /// <returns>
        /// Une nouvelle fonction qui retourne le résultat depuis le cache si la combinaison
        /// d'arguments a déjà été rencontrée, ou invoque <paramref name="fn"/> et met le résultat en cache sinon.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Le cache est thread-safe — implémenté via <see cref="ConcurrentDictionary{TKey,TValue}"/>.
        /// </para>
        /// <para>
        /// Les arguments sont combinés en un tuple struct comme clé,
        /// garantissant l'absence d'allocation heap par appel et satisfaisant nativement
        /// la contrainte <c>notnull</c> du dictionnaire — même si les arguments individuels
        /// sont <see langword="null"/>.
        /// </para>
        /// <para>
        /// Sur <c>netstandard2.0</c> et <c>netstandard2.1</c>, chaque argument est encapsulé
        /// dans <see cref="MayBe{T}"/> pour un support uniforme des valeurs <see langword="null"/>.
        /// </para>
        /// <para>
        /// Le cache est illimité — ne pas utiliser avec un grand nombre de combinaisons
        /// d'arguments distinctes sous peine de pression mémoire.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var add = Function.Memoize((int a, int b) => a + b);
        /// add(1, 2); // calcul effectué, résultat mis en cache
        /// add(1, 2); // retourné depuis le cache
        /// add(1, 3); // nouveau calcul
        /// </code>
        /// </example>
#if NET5_0_OR_GREATER
        public static Func<TArg1, TArg2, TResult> Memoize<TArg1, TArg2, TResult>(
            Func<TArg1, TArg2, TResult> fn)
        {
            var core = MemoizeCore<(TArg1, TArg2), TResult>(t => fn(t.Item1, t.Item2));
            return (arg1, arg2) => core((arg1, arg2));
        }
#else
        public static Func<TArg1, TArg2, TResult> Memoize<TArg1, TArg2, TResult>(
            Func<TArg1, TArg2, TResult> fn)
        {
            var cache = new Lazy<ConcurrentDictionary<(MayBe<TArg1>, MayBe<TArg2>), TResult>>(() => []);
            return (arg1, arg2) => GetOrAddLegacy(
                cache,
                ((MayBe<TArg1>)arg1, (MayBe<TArg2>)arg2),
                () => fn(arg1, arg2));
        }
#endif

        /// <summary>
        /// Retourne une version mémoïsée de <paramref name="fn"/> qui met en cache
        /// ses résultats selon les 3 arguments reçus.
        /// </summary>
        /// <typeparam name="TArg1">Type du premier argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TArg2">Type du deuxième argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TArg3">Type du troisième argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TResult">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction pure à mémoïser.</param>
        /// <returns>
        /// Une nouvelle fonction qui retourne le résultat depuis le cache si la combinaison
        /// d'arguments a déjà été rencontrée, ou invoque <paramref name="fn"/> et met le résultat en cache sinon.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Le cache est thread-safe — implémenté via <see cref="ConcurrentDictionary{TKey,TValue}"/>.
        /// </para>
        /// <para>
        /// Les arguments sont combinés en un tuple struct comme clé,
        /// garantissant l'absence d'allocation heap par appel et satisfaisant nativement
        /// la contrainte <c>notnull</c> du dictionnaire — même si les arguments individuels
        /// sont <see langword="null"/>.
        /// </para>
        /// <para>
        /// Sur <c>netstandard2.0</c> et <c>netstandard2.1</c>, chaque argument est encapsulé
        /// dans <see cref="MayBe{T}"/> pour un support uniforme des valeurs <see langword="null"/>.
        /// </para>
        /// <para>
        /// Le cache est illimité — ne pas utiliser avec un grand nombre de combinaisons
        /// d'arguments distinctes sous peine de pression mémoire.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var combine = Function.Memoize((string a, string b, string sep) => a + sep + b);
        /// combine("hello", "world", " "); // calcul effectué, résultat mis en cache
        /// combine("hello", "world", " "); // retourné depuis le cache
        /// combine("hello", "world", "-"); // nouveau calcul
        /// </code>
        /// </example>
#if NET5_0_OR_GREATER
        public static Func<TArg1, TArg2, TArg3, TResult> Memoize<TArg1, TArg2, TArg3, TResult>(
            Func<TArg1, TArg2, TArg3, TResult> fn)
        {
            var core = MemoizeCore<(TArg1, TArg2, TArg3), TResult>(
                t => fn(t.Item1, t.Item2, t.Item3));
            return (arg1, arg2, arg3) => core((arg1, arg2, arg3));
        }
#else
        public static Func<TArg1, TArg2, TArg3, TResult> Memoize<TArg1, TArg2, TArg3, TResult>(
            Func<TArg1, TArg2, TArg3, TResult> fn)
        {
            var cache = new Lazy<ConcurrentDictionary<(MayBe<TArg1>, MayBe<TArg2>, MayBe<TArg3>), TResult>>(() => []);
            return (arg1, arg2, arg3) => GetOrAddLegacy(
                cache,
                ((MayBe<TArg1>)arg1, (MayBe<TArg2>)arg2, (MayBe<TArg3>)arg3),
                () => fn(arg1, arg2, arg3));
        }
#endif
    }
}