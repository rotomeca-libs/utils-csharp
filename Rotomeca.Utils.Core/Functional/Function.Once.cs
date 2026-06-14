using Rotomeca.Utils;

namespace Rotomeca.Utils.Functional
{
    internal readonly struct Unit { }

    public static partial class Function
    {
        /// <summary>
        /// Retourne une version de <paramref name="fn"/> qui ne s'exécute qu'une seule fois.
        /// </summary>
        /// <typeparam name="TResult">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction à n'exécuter qu'une seule fois.</param>
        /// <returns>
        /// Une nouvelle fonction qui délègue à <paramref name="fn"/> uniquement au premier appel,
        /// puis retourne le résultat mis en cache pour tous les appels suivants.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Implémenté via <see cref="Lazy{T}"/> avec <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/> —
        /// thread-safe et libère automatiquement la référence à <paramref name="fn"/> après le premier appel.
        /// </para>
        /// <para>
        /// Contrairement à <see cref="Memoize{TResult}(Func{TResult})"/> qui partage la même sémantique
        /// pour les fonctions sans argument, <see cref="Once{TResult}"/> est l'expression d'une intention
        /// différente : initialisation unique plutôt que mise en cache de résultats.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var initApp = Function.Once(() => {
        ///     Console.WriteLine("Initialisation...");
        ///     return CreateApp();
        /// });
        ///
        /// initApp(); // exécute fn → "Initialisation..."
        /// initApp(); // retourne le même résultat, fn n'est plus appelée
        /// </code>
        /// </example>
        /// <seealso cref="Memoize{TResult}(Func{TResult})"/>
        public static Func<TResult> Once<TResult>(Func<TResult> fn)
        {
            var lazy = new Lazy<TResult>(fn, LazyThreadSafetyMode.ExecutionAndPublication);
            return () => lazy.Value;
        }

#if NET9_0_OR_GREATER
        /// <summary>
        /// Implémentation interne unique du pattern "once" pour les fonctions à 3 arguments.
        /// Toutes les surcharges publiques de <see cref="Once{TArg1,TResult}"/> délèguent ici.
        /// </summary>
        /// <typeparam name="TArg1">Type du premier argument.</typeparam>
        /// <typeparam name="TArg2">Type du deuxième argument. Peut être <see cref="Unit"/> si non utilisé.</typeparam>
        /// <typeparam name="TArg3">Type du troisième argument. Peut être <see cref="Unit"/> si non utilisé.</typeparam>
        /// <typeparam name="TResult">Type du résultat.</typeparam>
        /// <param name="fn">Fonction à n'exécuter qu'une seule fois.</param>
        /// <returns>Une fonction thread-safe n'exécutant <paramref name="fn"/> qu'au premier appel.</returns>
        /// <remarks>
        /// <para>
        /// Thread-safety assurée par double-checked locking :
        /// <list type="bullet">
        /// <item><description>
        /// Chemin rapide (post-initialisation) : lecture volatile de la référence
        /// <see cref="Types.Internal.VolatileMayBe{T}"/> — aucun lock, aucune allocation.
        /// </description></item>
        /// <item><description>
        /// Premier appel : lock exclusif avec double vérification, écriture de la valeur,
        /// libération de <paramref name="fn"/> pour permettre la collecte GC de ses captures.
        /// </description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Sur .NET 9+, utilise <see cref="System.Threading.Lock"/> pour de meilleures performances.
        /// Sur les cibles antérieures, utilise un <see langword="object"/> standard.
        /// </para>
        /// </remarks>
        private static Func<TArg1, TArg2, TArg3, TResult> _Once<TArg1, TArg2, TArg3, TResult>(
        Func<TArg1, TArg2, TArg3, TResult> fn)
#else
        /// <summary>
        /// Implémentation interne unique du pattern "once" pour les fonctions à 3 arguments.
        /// Toutes les surcharges publiques de <see cref="Once{TArg1,TResult}"/> délèguent ici.
        /// </summary>
        /// <typeparam name="TArg1">Type du premier argument.</typeparam>
        /// <typeparam name="TArg2">Type du deuxième argument. Peut être <see cref="Unit"/> si non utilisé.</typeparam>
        /// <typeparam name="TArg3">Type du troisième argument. Peut être <see cref="Unit"/> si non utilisé.</typeparam>
        /// <typeparam name="TResult">Type du résultat.</typeparam>
        /// <param name="fn">Fonction à n'exécuter qu'une seule fois.</param>
        /// <returns>Une fonction thread-safe n'exécutant <paramref name="fn"/> qu'au premier appel.</returns>
        /// <remarks>
        /// <para>
        /// Thread-safety assurée par double-checked locking :
        /// <list type="bullet">
        /// <item><description>
        /// Chemin rapide (post-initialisation) : lecture volatile de la référence
        /// <see cref="Types.Internal.VolatileMayBe{T}"/> — aucun lock, aucune allocation.
        /// </description></item>
        /// <item><description>
        /// Premier appel : lock exclusif avec double vérification, écriture de la valeur,
        /// libération de <paramref name="fn"/> pour permettre la collecte GC de ses captures.
        /// </description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Sur .NET 9+, utilise <see href="https://learn.microsoft.com/fr-fr/dotnet/api/system.threading.lock?view=net-9.0">System.Threading.Lock</see> pour de meilleures performances.
        /// Sur les cibles antérieures, utilise un <see langword="object"/> standard.
        /// </para>
        /// </remarks>
        private static Func<TArg1, TArg2, TArg3, TResult> _Once<TArg1, TArg2, TArg3, TResult>(
            Func<TArg1, TArg2, TArg3, TResult> fn)
#endif
        {
            Types.Internal.VolatileMayBe<TResult>? result = null;
            Func<TArg1, TArg2, TArg3, TResult>? _fn = fn;
#if NET9_0_OR_GREATER
            Lock _lock = new();
#else
            object _lock = new();
#endif

            return (TArg1 a, TArg2 b, TArg3 c) =>
            {
                // Chemin rapide — acquire fence, aucun lock après initialisation
                var current = Volatile.Read(ref result);
                if (current.HasValue()) return current!.Value!;

                lock (_lock)
                {
                    // Double vérification — un autre thread a pu initialiser entre-temps
                    if (result.HasValue()) return result!.Value!;
                    result = _fn!(a, b, c);
                    _fn = null; // libère fn → le GC peut collecter ses captures
                }

                return result.Value!;
            };
        }

        /// <summary>
        /// Retourne une version de <paramref name="fn"/> qui ne s'exécute qu'une seule fois,
        /// quel que soit l'argument passé aux appels suivants.
        /// </summary>
        /// <typeparam name="TArg1">Type de l'argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TResult">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction à n'exécuter qu'une seule fois.</param>
        /// <returns>
        /// Une nouvelle fonction qui exécute <paramref name="fn"/> au premier appel
        /// et retourne ce résultat pour tous les appels suivants.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Délègue à <c>_Once&lt;TArg1, Unit, Unit, TResult&gt;</c> en passant des arguments
        /// <see cref="Unit"/> (struct vide) pour les positions inutilisées.
        /// Le JIT élimine ces paramètres fantômes — surcoût nul à l'appel.
        /// </para>
        /// <para>
        /// Les deux closures de wrapping (<c>(a,_,_) =&gt; fn(a)</c> et <c>a =&gt; inner(a, default, default)</c>)
        /// sont allouées <b>une seule fois</b> lors de l'appel à <c>Once(fn)</c>, jamais lors des invocations.
        /// </para>
        /// </remarks>
        /// <seealso cref="Once{TResult}(Func{TResult})"/>
        /// <seealso cref="Memoize{TArg1,TResult}(Func{TArg1,TResult})"/>
        public static Func<TArg1, TResult> Once<TArg1, TResult>(Func<TArg1, TResult> fn)
        {
            var inner = _Once<TArg1, Unit, Unit, TResult>((a, _, _) => fn(a));
            return a => inner(a, default, default);
        }

        /// <summary>
        /// Retourne une version de <paramref name="fn"/> qui ne s'exécute qu'une seule fois,
        /// quels que soient les arguments passés aux appels suivants.
        /// </summary>
        /// <typeparam name="TArg1">Type du premier argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TArg2">Type du deuxième argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TResult">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction à n'exécuter qu'une seule fois.</param>
        /// <returns>
        /// Une nouvelle fonction qui exécute <paramref name="fn"/> au premier appel
        /// et retourne ce résultat pour tous les appels suivants.
        /// </returns>
        /// <remarks>
        /// Délègue à <c>_Once&lt;TArg1, TArg2, Unit, TResult&gt;</c>.
        /// Surcoût d'une closure de wrapping allouée uniquement à la création.
        /// </remarks>
        /// <seealso cref="Once{TResult}(Func{TResult})"/>
        /// <seealso cref="Memoize{TArg1,TArg2,TResult}(Func{TArg1,TArg2,TResult})"/>
        public static Func<TArg1, TArg2, TResult> Once<TArg1, TArg2, TResult>(
            Func<TArg1, TArg2, TResult> fn)
        {
            var inner = _Once<TArg1, TArg2, Unit, TResult>((a, b, _) => fn(a, b));
            return (a, b) => inner(a, b, default);
        }

        /// <summary>
        /// Retourne une version de <paramref name="fn"/> qui ne s'exécute qu'une seule fois,
        /// quels que soient les arguments passés aux appels suivants.
        /// </summary>
        /// <typeparam name="TArg1">Type du premier argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TArg2">Type du deuxième argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TArg3">Type du troisième argument de <paramref name="fn"/>.</typeparam>
        /// <typeparam name="TResult">Type du résultat retourné par <paramref name="fn"/>.</typeparam>
        /// <param name="fn">Fonction à n'exécuter qu'une seule fois.</param>
        /// <returns>
        /// Une nouvelle fonction qui exécute <paramref name="fn"/> au premier appel
        /// et retourne ce résultat pour tous les appels suivants.
        /// </returns>
        /// <remarks>
        /// Délègue directement à <c>_Once</c> sans wrapping supplémentaire —
        /// c'est la surcharge la plus efficace à la création.
        /// </remarks>
        /// <seealso cref="Once{TResult}(Func{TResult})"/>
        /// <seealso cref="Memoize{TArg1,TArg2,TArg3,TResult}(Func{TArg1,TArg2,TArg3,TResult})"/>
        public static Func<TArg1, TArg2, TArg3, TResult> Once<TArg1, TArg2, TArg3, TResult>(
            Func<TArg1, TArg2, TArg3, TResult> fn) => _Once(fn);
    }

}
