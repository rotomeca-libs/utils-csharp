using Rotomeca.Utils.Types;
using System.Collections.Concurrent;

namespace Rotomeca.Utils.Functional
{
    public static partial class Function
    {
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
        /// <see langword="null"/> — sont encapsulées dans <see cref="MayBe{T}"/> pour contourner
        /// la contrainte <c>notnull</c> absente sur ces targets.
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
            TResult[]? argNullResult = null;
#pragma warning disable CS8714
            ConcurrentDictionary<TArg1, TResult>? cache = null;
#pragma warning restore CS8714

            return (TArg1 arg) =>
            {
                if (arg is null) return (argNullResult ??= [fn(arg)])[0];

                if ((cache ??= []).TryGetValue(arg, out TResult? value)) return value!;

                var result = fn(arg);
                cache![arg] = result;
                return result;
            };
        }
#else
        public static Func<TArg1, TResult> Memoize<TArg1,TResult>(Func<TArg1,TResult> fn)
        {
            MayBe<ConcurrentDictionary<MayBe<TArg1>, TResult>> cache = MayBe<ConcurrentDictionary<MayBe<TArg1>, TResult>>.Null;

            return (TArg1 arg) =>
            {
                if (cache.IsEmpty) cache = MayBe<ConcurrentDictionary<MayBe<TArg1>, TResult>>.Some([]);

                if (cache.Value!.TryGetValue(arg, out TResult value)) return value;

                var result = fn(arg);
                cache.Value[arg] = result;
                return result;
            };
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
        /// Les arguments sont combinés en un tuple struct <c>(TArg1, TArg2)</c> comme clé,
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
        public static Func<TArg1, TArg2, TResult> Memoize<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> fn)
        {
            ConcurrentDictionary<(TArg1, TArg2), TResult>? cache = null;

            return (TArg1 arg1, TArg2 arg2) =>
            {
                var key = (arg1, arg2);
                if ((cache ??= []).TryGetValue(key, out TResult? value)) return value!;

                var result = fn(arg1, arg2);
                cache![key] = result;
                return result;
            };
        }
#else
        public static Func<TArg1, TArg2, TResult> Memoize<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> fn)
        {
            MayBe<ConcurrentDictionary<(MayBe<TArg1>, MayBe<TArg2>), TResult>> cache
                = MayBe<ConcurrentDictionary<(MayBe<TArg1>, MayBe<TArg2>), TResult>>.Null;
 
            return (TArg1 arg1, TArg2 arg2) =>
            {
                if (cache.IsEmpty)
                    cache = MayBe<ConcurrentDictionary<(MayBe<TArg1>, MayBe<TArg2>), TResult>>.Some([]);
 
                // Conversion implicite TArg → MayBe<TArg> pour support uniforme de null
                var key = ((MayBe<TArg1>)arg1, (MayBe<TArg2>)arg2);
                if (cache.Value!.TryGetValue(key, out TResult value)) return value;
 
                var result = fn(arg1, arg2);
                cache.Value[key] = result;
                return result;
            };
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
        /// Les arguments sont combinés en un tuple struct <c>(TArg1, TArg2, TArg3)</c> comme clé,
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
        public static Func<TArg1, TArg2, TArg3, TResult> Memoize<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> fn)
        {
            ConcurrentDictionary<(TArg1, TArg2, TArg3), TResult>? cache = null;

            return (TArg1 arg1, TArg2 arg2, TArg3 arg3) =>
            {
                var key = (arg1, arg2, arg3);
                if ((cache ??= []).TryGetValue(key, out TResult? value)) return value!;

                var result = fn(arg1, arg2, arg3);
                cache![key] = result;
                return result;
            };
        }
#else
        public static Func<TArg1, TArg2, TArg3, TResult> Memoize<TArg1, TArg2, TArg3, TResult>(Func<TArg1, TArg2, TArg3, TResult> fn)
        {
            MayBe<ConcurrentDictionary<(MayBe<TArg1>, MayBe<TArg2>, MayBe<TArg3>), TResult>> cache
                = MayBe<ConcurrentDictionary<(MayBe<TArg1>, MayBe<TArg2>, MayBe<TArg3>), TResult>>.Null;
 
            return (TArg1 arg1, TArg2 arg2, TArg3 arg3) =>
            {
                if (cache.IsEmpty)
                    cache = MayBe<ConcurrentDictionary<(MayBe<TArg1>, MayBe<TArg2>, MayBe<TArg3>), TResult>>.Some([]);
 
                // Conversion implicite TArg → MayBe<TArg> pour support uniforme de null
                var key = ((MayBe<TArg1>)arg1, (MayBe<TArg2>)arg2, (MayBe<TArg3>)arg3);
                if (cache.Value!.TryGetValue(key, out TResult value)) return value;
 
                var result = fn(arg1, arg2, arg3);
                cache.Value[key] = result;
                return result;
            };
        }
#endif
    }
}
