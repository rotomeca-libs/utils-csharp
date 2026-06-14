using Rotomeca.Core.Collections;
using Rotomeca.Core.Optionals;
using System.Collections;
using System.Numerics;

namespace Rotomeca.Utils.Collections
{
    /// <summary>
    /// Fournit des utilitaires tableaux inspirés des API JavaScript,
    /// portés pour un usage idiomatique en C#.
    /// </summary>
    public static partial class Arrays
    {
        /// <summary>
        /// Divise un tableau en plusieurs sous-tableaux (chunks) d'une taille donnée.
        /// Le dernier chunk peut être plus petit si la taille du tableau n'est pas
        /// un multiple de <paramref name="size"/>.
        /// </summary>
        /// <typeparam name="T">Le type des éléments du tableau.</typeparam>
        /// <param name="original">Le tableau source à découper.</param>
        /// <param name="size">La taille maximale de chaque chunk.</param>
        /// <returns>
        /// Un tableau de sous-tableaux de type <typeparamref name="T"/>.
        /// Retourne un tableau vide si <paramref name="size"/> vaut 0.
        /// </returns>
        /// <example>
        /// <code>
        /// var source = new RArray&lt;int&gt;(1, 2, 3, 4, 5);
        /// RArray&lt;RArray&lt;int&gt;&gt; chunks = source.Chunck(2);
        /// // chunks => [[1, 2], [3, 4], [5]]
        /// </code>
        /// </example>
        public static RArray<RArray<T>> Chunk<T>(this RArray<T> original, uint size)
        {
            if (size == 0) return [];

#if NET6_0_OR_GREATER
            return original.Chunk((int)size).Select(c => new RArray<T>(c)).ToRArray();
#else
            RArray<T> array = new(original);
            var length = array.Length;

            // Calcule le nombre de chunks nécessaires (arrondi à l'entier supérieur)
            var chunkCount = Math.Ceiling(length / (double)size);
            var result = new List<RArray<T>>((int)chunkCount);

            var arrIndex = 0;

            while (arrIndex < length)
            {
                // Le dernier chunk peut être plus petit que size
                var currentChunkSize = Math.Min(size, length - arrIndex);
                var subArray = new T[currentChunkSize];

                for (int i = 0; i < currentChunkSize; ++i)
                {
                    subArray[i] = array[arrIndex + i];
                }

                result.Add(new(subArray));
                arrIndex += (int)size;
            }

            return new(result);
#endif
        }

        /// <summary>
        /// Retourne un tableau ne contenant que des éléments distincts,
        /// en utilisant le comparateur d'égalité par défaut pour le type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Le type des éléments du tableau.</typeparam>
        /// <param name="array">Le tableau source.</param>
        /// <returns>
        /// Un nouveau tableau sans doublons, dans l'ordre d'apparition des éléments.
        /// </returns>
        /// <example>
        /// <code>
        /// int[] source = [1, 2, 2, 3, 1, 4];
        /// RArray&lt;int&gt; unique = source.Unique();
        /// // unique => [1, 2, 3, 4]
        /// </code>
        /// </example>
        public static RArray<T> Unique<T>(this IEnumerable<T> array) => array.Distinct().ToRArray();

        /// <summary>
        /// Retourne un tableau ne contenant que des éléments distincts,
        /// en se basant sur une clé extraite par <paramref name="fn"/>.
        /// En cas de doublons, le premier élément rencontré est conservé.
        /// </summary>
        /// <typeparam name="T">Le type des éléments du tableau.</typeparam>
        /// <typeparam name="TResult">
        /// Le type de la clé utilisée pour comparer les éléments.
        /// Doit être non-nullable.
        /// </typeparam>
        /// <param name="original">Le tableau source.</param>
        /// <param name="fn">Fonction qui extrait la clé de comparaison depuis un élément.</param>
        /// <returns>
        /// Un nouveau tableau contenant uniquement les éléments dont la clé est unique.
        /// </returns>
        /// <example>
        /// <code>
        /// var people = new[] {
        ///     new Person("Alice", 30),
        ///     new Person("Bob",   25),
        ///     new Person("Alice", 42),
        /// };
        ///
        /// RArray&lt;Person&gt; unique = people.UniqueBy(p => p.Name);
        /// // unique => [{ "Alice", 30 }, { "Bob", 25 }]
        /// </code>
        /// </example>
        public static RArray<T> UniqueBy<T, TResult>(this IEnumerable<T> original, Func<T, TResult> fn) where TResult : notnull
        {
#if NET6_0_OR_GREATER
            return original.DistinctBy(fn).ToRArray();
#else
            var array = new RArray<T>(original);
            var length = array.Length;
            Dictionary<TResult, object?> seen = [];
            List<T> result = [];

            for (int i = 0; i < length; ++i)
            {
                var item = array[i];
                var key = fn(item);

                if (!seen.ContainsKey(key))
                {
                    seen.Add(key, null);
                    result.Add(item);
                }
            }

            return result.ToRArray();
#endif
        }

        /// <summary>
        /// Regroupe les éléments d'une séquence par une clé extraite via <paramref name="fn"/>,
        /// et retourne un dictionnaire associant chaque clé à un tableau des éléments correspondants.
        /// </summary>
        /// <typeparam name="TKey">Le type de la clé de regroupement. Doit être non-nullable.</typeparam>
        /// <typeparam name="T">Le type des éléments de la séquence.</typeparam>
        /// <param name="original">La séquence source à regrouper.</param>
        /// <param name="fn">Fonction qui extrait la clé de regroupement depuis un élément.</param>
        /// <returns>
        /// Un dictionnaire où chaque clé est associée à un <see cref="RArray{T}"/>
        /// contenant tous les éléments du groupe correspondant.
        /// </returns>
        /// <example>
        /// <code>
        /// var words = new[] { "apple", "avocado", "banana", "blueberry" };
        /// var byLetter = words.GroupTo(w => w[0]);
        /// // byLetter['a'] => ["apple", "avocado"]
        /// // byLetter['b'] => ["banana", "blueberry"]
        /// </code>
        /// </example>
        public static Dictionary<TKey, RArray<T>> GroupTo<TKey, T>(this IEnumerable<T> original, Func<T, TKey> fn) where TKey : notnull
        {
            Dictionary<TKey, RArray<T>> result = [];

            foreach (var item in original.GroupBy(fn))
            {
                result.Add(item.Key, item.ToRArray());
            }

            return result;
        }

        /// <summary>
        /// Retourne le premier élément de la séquence enveloppé dans un <see cref="MayBe{T}"/>,
        /// ou <see cref="MayBe{T}.Null"/> si la séquence est vide.
        /// </summary>
        /// <typeparam name="T">Le type des éléments de la séquence.</typeparam>
        /// <param name="values">La séquence source.</param>
        /// <returns>
        /// Un <see cref="MayBe{T}"/> contenant le premier élément,
        /// ou <see cref="MayBe{T}.Null"/> si la séquence est vide.
        /// </returns>
        /// <example>
        /// <code>
        /// RArray&lt;int&gt; filled = new (10, 20, 30);
        /// MayBe&lt;int&gt; first = filled.First(); // HasValue = true, Value = 10
        /// int implicitFirst = first; // Cast implicit
        ///
        /// RArray&lt;int&gt; empty = new ();
        /// MayBe&lt;int&gt; none = empty.First(); // HasValue = false
        /// </code>
        /// </example>
        public static MayBe<T> First<T>(this RArray<T> values)
        {
            if (values.Length != 0) return Enumerable.First(values);
            else return MayBe<T>.Null;
        }

        /// <summary>
        /// Retourne le dernier élément de la séquence enveloppé dans un <see cref="MayBe{T}"/>,
        /// ou <see cref="MayBe{T}.Null"/> si la séquence est vide.
        /// </summary>
        /// <typeparam name="T">Le type des éléments de la séquence.</typeparam>
        /// <param name="values">La séquence source.</param>
        /// <returns>
        /// Un <see cref="MayBe{T}"/> contenant le dernier élément,
        /// ou <see cref="MayBe{T}.Null"/> si la séquence est vide.
        /// </returns>
        /// <example>
        /// <code>
        /// RArray&lt;int&gt; filled = new (10, 20, 30);
        /// MayBe&lt;int&gt; last = filled.Last(); // HasValue = true, Value = 30
        /// int implicitLast = last; // Cast implicit
        ///
        /// RArray&lt;int&gt; empty = new ();
        /// MayBe&lt;int&gt; none = empty.Last(); // HasValue = false
        /// </code>
        /// </example>
        public static MayBe<T> Last<T>(this RArray<T> values)
        {
            if (values.Length != 0) return Enumerable.Last(values);
            else return MayBe<T>.Null;
        }

#if NET7_0_OR_GREATER
        /// <summary>
        /// Calcule la somme de tous les éléments de la séquence.
        /// </summary>
        /// <typeparam name="T">
        /// Le type des éléments. Doit implémenter <see cref="INumber{T}"/>.
        /// </typeparam>
        /// <param name="values">La séquence source.</param>
        /// <returns>La somme de tous les éléments, ou <c>T.Zero</c> si la séquence est vide.</returns>
        /// <example>
        /// <code>
        /// int[] nums = [1, 2, 3, 4];
        /// int total = nums.Sum(); // 10
        /// </code>
        /// </example>
        public static T Sum<T>(this IEnumerable<T> values) where T : INumber<T>
            => values.Aggregate(T.Zero, (a, b) => a + b);
#else
        /// <summary>
        /// Calcule la somme de tous les éléments de la séquence
        /// via l'interface <see cref="Interfaces.IAggregable{T}"/>.
        /// </summary>
        /// <typeparam name="T">Le type des éléments. Doit implémenter <see cref="Interfaces.IAggregable{T}"/>.</typeparam>
        /// <param name="values">La séquence source.</param>
        /// <returns>La somme de tous les éléments.</returns>
        public static T Sum<T>(this IEnumerable<T> values) where T : Interfaces.IAggregable<T> => values.Aggregate(default(T)!, (a, b) => a.Add(b));

        /// <inheritdoc cref="Sum{T}(IEnumerable{T})"/>
        public static int Sum(this IEnumerable<int> values) => values.Aggregate(0, (a, b) => a + b);

        /// <inheritdoc cref="Sum{T}(IEnumerable{T})"/>
        public static long Sum(this IEnumerable<long> values) => values.Aggregate(0L, (a, b) => a + b);

        /// <inheritdoc cref="Sum{T}(IEnumerable{T})"/>
        public static float Sum(this IEnumerable<float> values) => values.Aggregate(0f, (a, b) => a + b);

        /// <inheritdoc cref="Sum{T}(IEnumerable{T})"/>
        public static double Sum(this IEnumerable<double> values) => values.Aggregate(0.0, (a, b) => a + b);

        /// <inheritdoc cref="Sum{T}(IEnumerable{T})"/>
        public static decimal Sum(this IEnumerable<decimal> values) => values.Aggregate(0m, (a, b) => a + b);
#endif

        /// <summary>
        /// Trie les éléments d'une séquence par ordre croissant selon une clé extraite
        /// par <paramref name="fn"/>.
        /// </summary>
        /// <typeparam name="T">Le type des éléments de la séquence.</typeparam>
        /// <typeparam name="TSort">Le type de la clé de tri.</typeparam>
        /// <param name="value">La séquence source.</param>
        /// <param name="fn">Fonction qui extrait la clé de tri depuis un élément.</param>
        /// <returns>Un nouveau tableau trié par ordre croissant de la clé.</returns>
        /// <example>
        /// <code>
        /// var people = new[] { new Person("Charlie", 35), new Person("Alice", 28) };
        /// var sorted = people.SortBy(p => p.Name);
        /// // sorted => [{ "Alice", 28 }, { "Charlie", 35 }]
        /// </code>
        /// </example>
        public static RArray<T> SortBy<T, TSort>(this IEnumerable<T> value, Func<T, TSort> fn) => value.OrderBy(fn).ToRArray();

        /// <summary>
        /// Aplatit une séquence de séquences en un tableau unique (un seul niveau de profondeur).
        /// </summary>
        /// <typeparam name="T">Le type des éléments des séquences imbriquées.</typeparam>
        /// <param name="value">La séquence de séquences à aplatir.</param>
        /// <returns>Un tableau contenant tous les éléments des séquences imbriquées, dans l'ordre.</returns>
        /// <example>
        /// <code>
        /// int[][] nested = [[1, 2], [3, 4], [5]];
        /// RArray&lt;int&gt; flat = nested.Flatten();
        /// // flat => [1, 2, 3, 4, 5]
        /// </code>
        /// </example>
        public static RArray<T> Flatten<T>(this IEnumerable<IEnumerable<T>> value) => value.SelectMany(x => x).ToRArray();

        /// <summary>
        /// Aplatit récursivement une structure imbriquée à profondeur arbitraire
        /// en un tableau de type <typeparamref name="T"/>.
        /// Tout élément qui n'est pas de type <typeparamref name="T"/> mais implémente
        /// <see cref="IEnumerable"/> est parcouru récursivement.
        /// </summary>
        /// <typeparam name="T">Le type des éléments à collecter dans le résultat.</typeparam>
        /// <param name="values">La structure imbriquée source (non générique).</param>
        /// <returns>Un tableau plat contenant tous les éléments de type <typeparamref name="T"/> trouvés.</returns>
        /// <example>
        /// <code>
        /// object[] nested = [1, new object[] { 2, new object[] { 3, 4 } }, 5];
        /// RArray&lt;int&gt; flat = nested.FlattenDeep&lt;int&gt;();
        /// // flat => [1, 2, 3, 4, 5]
        /// </code>
        /// </example>
        public static RArray<T> FlattenDeep<T>(this IEnumerable values)
        {
            RArray<T> result = [];

            foreach (var item in values)
            {
                if (item is T value) result.Push(value);
                else if (item is IEnumerable innerValues) result.Push(FlattenDeep<T>(innerValues));
            }

            return result;
        }

        /// <summary>
        /// Supprime les éléments <see langword="null"/> d'une séquence de types référence.
        /// </summary>
        /// <typeparam name="T">Le type référence des éléments.</typeparam>
        /// <param name="values">La séquence source pouvant contenir des valeurs nulles.</param>
        /// <returns>Un nouveau tableau ne contenant aucun élément <see langword="null"/>.</returns>
        /// <example>
        /// <code>
        /// string?[] source = ["hello", null, "world", null];
        /// RArray&lt;string&gt; compact = source.Compact();
        /// // compact => ["hello", "world"]
        /// </code>
        /// </example>
        public static RArray<T> Compact<T>(this IEnumerable<T> values) where T : class
            => values.Where(v => v != null).ToRArray();

        /// <summary>
        /// Extrait les valeurs présentes d'une séquence de <see cref="MayBe{T}"/>,
        /// en ignorant les éléments vides (<see cref="MayBe{T}.Null"/>).
        /// </summary>
        /// <typeparam name="T">Le type de la valeur encapsulée.</typeparam>
        /// <param name="values">La séquence de <see cref="MayBe{T}"/> source.</param>
        /// <returns>Un nouveau tableau contenant uniquement les valeurs des éléments non vides.</returns>
        /// <example>
        /// <code>
        /// MayBe&lt;int&gt;[] source = [MayBe&lt;int&gt;.Null, 1, MayBe&lt;int&gt;.Null, 2, 3];
        /// RArray&lt;int&gt; compact = source.Compact();
        /// // compact => [1, 2, 3]
        /// </code>
        /// </example>
        public static RArray<T> Compact<T>(this IEnumerable<MayBe<T>> values)
            => values.Where(v => v.HasValue).Select(x => x.Value!).ToRArray();

        /// <summary>
        /// Divise une séquence en deux groupes selon un prédicat :
        /// les éléments satisfaisant le prédicat (<c>truthy</c>)
        /// et ceux ne le satisfaisant pas (<c>falsy</c>).
        /// </summary>
        /// <typeparam name="T">Le type des éléments de la séquence.</typeparam>
        /// <param name="values">La séquence source.</param>
        /// <param name="predicate">La condition de partitionnement.</param>
        /// <returns>
        /// Un tuple contenant deux tableaux : <c>truthy</c> pour les éléments
        /// validant le prédicat, <c>falsy</c> pour les autres.
        /// </returns>
        /// <example>
        /// <code>
        /// int[] nums = [1, 2, 3, 4, 5, 6];
        /// var (evens, odds) = nums.Partition(n => n % 2 == 0);
        /// // evens => [2, 4, 6]
        /// // odds  => [1, 3, 5]
        /// </code>
        /// </example>
        public static (RArray<T> truthy, RArray<T> falsy) Partition<T>(this IEnumerable<T> values, Func<T, bool> predicate)
        {
            RArray<T> truthy = [];
            RArray<T> falsy = [];
            foreach (var item in values)
            {
                if (predicate(item)) truthy.Push(item);
                else falsy.Push(item);
            }
            return (truthy, falsy);
        }

        /// <summary>
        /// Divise une séquence en deux groupes selon un prédicat et retourne le résultat
        /// sous forme d'un <see cref="Types.JsObject"/> avec les clés <c>"truthy"</c> et <c>"falsy"</c>.
        /// </summary>
        /// <typeparam name="T">Le type des éléments de la séquence.</typeparam>
        /// <param name="values">La séquence source.</param>
        /// <param name="predicate">La condition de partitionnement.</param>
        /// <returns>
        /// Un <see cref="Types.JsObject"/> contenant les entrées <c>"truthy"</c> et <c>"falsy"</c>,
        /// chacune associée à un <see cref="RArray{T}"/>.
        /// </returns>
        /// <seealso cref="Partition{T}(IEnumerable{T}, Func{T, bool})"/>
        public static Types.JsObject PartitionToObject<T>(this IEnumerable<T> values, Func<T, bool> predicate)
        {
            var (truthy, falsy) = values.Partition(predicate);
            return new Types.JsObject
            {
                ["truthy"] = truthy,
                ["falsy"] = falsy
            };
        }

        /// <summary>
        /// Retourne les éléments présents à la fois dans <paramref name="values"/>
        /// et dans <paramref name="others"/> (intersection ensembliste A ∩ B).
        /// Les doublons sont éliminés.
        /// </summary>
        /// <typeparam name="T">Le type des éléments.</typeparam>
        /// <param name="values">La première séquence.</param>
        /// <param name="others">La seconde séquence.</param>
        /// <returns>Un tableau contenant les éléments communs aux deux séquences.</returns>
        /// <example>
        /// <code>
        /// int[] a = [1, 2, 3, 4];
        /// int[] b = [3, 4, 5, 6];
        /// RArray&lt;int&gt; inter = a.Intersection(b);
        /// // inter => [3, 4]
        /// </code>
        /// </example>
        public static RArray<T> Intersection<T>(this IEnumerable<T> values, IEnumerable<T> others) => values.Intersect(others).ToRArray();

        /// <summary>
        /// Retourne les éléments présents dans <paramref name="values"/>
        /// mais absents de <paramref name="others"/> (différence ensembliste A \ B).
        /// Les doublons sont éliminés.
        /// </summary>
        /// <typeparam name="T">Le type des éléments.</typeparam>
        /// <param name="values">La séquence source.</param>
        /// <param name="others">La séquence des éléments à exclure.</param>
        /// <returns>Un tableau contenant les éléments de <paramref name="values"/> non présents dans <paramref name="others"/>.</returns>
        /// <example>
        /// <code>
        /// int[] a = [1, 2, 3, 4];
        /// int[] b = [3, 4, 5, 6];
        /// RArray&lt;int&gt; diff = a.Difference(b);
        /// // diff => [1, 2]
        /// </code>
        /// </example>
        public static RArray<T> Difference<T>(this IEnumerable<T> values, IEnumerable<T> others) => values.Except(others).ToRArray();

        /// <summary>
        /// Retourne l'union des deux séquences : tous les éléments distincts
        /// présents dans l'une ou l'autre (union ensembliste A ∪ B).
        /// Les doublons sont éliminés.
        /// </summary>
        /// <typeparam name="T">Le type des éléments.</typeparam>
        /// <param name="values">La première séquence.</param>
        /// <param name="others">La seconde séquence.</param>
        /// <returns>Un tableau contenant tous les éléments uniques des deux séquences.</returns>
        /// <example>
        /// <code>
        /// RArray&lt;int&gt; a = new (1, 2, 3);
        /// int[] b = [3, 4, 5];
        /// RArray&lt;int&gt; union = a.Union(b);
        /// // union => [1, 2, 3, 4, 5]
        /// </code>
        /// </example>
        public static RArray<T> Union<T>(this RArray<T> values, IEnumerable<T> others) => Enumerable.Union(values, others).Unique();

#if NETSTANDARD2_0 || NETSTANDARD2_1
        /// <summary>
        /// Associe les éléments de deux séquences deux-à-deux en tuples,
        /// jusqu'à la longueur de la plus courte des deux.
        /// </summary>
        /// <typeparam name="T">Le type des éléments de la première séquence.</typeparam>
        /// <typeparam name="Y">Le type des éléments de la seconde séquence.</typeparam>
        /// <param name="a">La première séquence.</param>
        /// <param name="b">La seconde séquence.</param>
        /// <returns>
        /// Un tableau de tuples <c>(T, Y)</c>, de longueur égale au minimum
        /// des longueurs de <paramref name="a"/> et <paramref name="b"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// RArray&lt;int&gt; nums  = [1, 2, 3];
        /// string[] words = ["un", "deux"];
        /// var zipped = nums.Zip(words);
        /// // zipped => [(1, "un"), (2, "deux")]
        /// </code>
        /// </example>
        public static RArray<(T, Y)> Zip<T, Y>(this RArray<T> a, IEnumerable<Y> b)
            => Enumerable.Zip(a, b, (x, y) => (x, y)).ToRArray();
#else
        /// <summary>
        /// Associe les éléments de deux séquences deux-à-deux en tuples,
        /// jusqu'à la longueur de la plus courte des deux.
        /// </summary>
        /// <typeparam name="T">Le type des éléments de la première séquence.</typeparam>
        /// <typeparam name="Y">Le type des éléments de la seconde séquence.</typeparam>
        /// <param name="a">La première séquence.</param>
        /// <param name="b">La seconde séquence.</param>
        /// <returns>
        /// Un tableau de tuples <c>(T, Y)</c>, de longueur égale au minimum
        /// des longueurs de <paramref name="a"/> et <paramref name="b"/>.
        /// </returns>
        /// <example>
        /// <code>
        /// RArray&lt;int&gt; nums  = [1, 2, 3];
        /// string[] words = ["un", "deux"];
        /// var zipped = nums.Zip(words);
        /// // zipped => [(1, "un"), (2, "deux")]
        /// </code>
        /// </example>
        public static RArray<(T, Y)> Zip<T, Y>(this RArray<T> a, IEnumerable<Y> b)
            => Enumerable.Zip(a, b).ToRArray();
#endif

        /// <summary>
        /// Retourne les <paramref name="nb"/> premiers éléments de la séquence.
        /// </summary>
        /// <typeparam name="T">Le type des éléments.</typeparam>
        /// <param name="a">La séquence source.</param>
        /// <param name="nb">Le nombre d'éléments à conserver depuis le début.</param>
        /// <returns>
        /// Un tableau contenant au plus <paramref name="nb"/> éléments.
        /// Si la séquence contient moins d'éléments, tous sont retournés.
        /// </returns>
        /// <example>
        /// <code>
        /// RArray&lt;int&gt; nums = new (1, 2, 3, 4, 5);
        /// RArray&lt;int&gt; first3 = nums.Take(3);
        /// // first3 => [1, 2, 3]
        /// </code>
        /// </example>
        public static RArray<T> Take<T>(this RArray<T> a, uint nb) => Enumerable.Take(a, (int)nb).ToRArray();

        /// <summary>
        /// Ignore les <paramref name="nb"/> premiers éléments de la séquence
        /// et retourne le reste.
        /// </summary>
        /// <typeparam name="T">Le type des éléments.</typeparam>
        /// <param name="a">La séquence source.</param>
        /// <param name="nb">Le nombre d'éléments à ignorer depuis le début.</param>
        /// <returns>
        /// Un tableau contenant tous les éléments après les <paramref name="nb"/> premiers.
        /// Retourne un tableau vide si <paramref name="nb"/> est supérieur ou égal à la longueur.
        /// </returns>
        /// <example>
        /// <code>
        /// int[] nums = [1, 2, 3, 4, 5];
        /// RArray&lt;int&gt; tail = nums.Drop(2);
        /// // tail => [3, 4, 5]
        /// </code>
        /// </example>
        public static RArray<T> Drop<T>(this IEnumerable<T> a, uint nb) => a.Skip((int)nb).ToRArray();

        /// <summary>
        /// Retourne un nouveau tableau dont les éléments sont dans un ordre aléatoire
        /// (algorithme de Fisher-Yates).
        /// </summary>
        /// <typeparam name="T">Le type des éléments.</typeparam>
        /// <param name="a">La séquence source.</param>
        /// <returns>Un nouveau tableau contenant les mêmes éléments dans un ordre aléatoire.</returns>
        /// <example>
        /// <code>
        /// int[] nums = [1, 2, 3, 4, 5];
        /// RArray&lt;int&gt; shuffled = nums.Shuffle();
        /// // shuffled => [3, 1, 5, 2, 4] (ordre aléatoire)
        /// </code>
        /// </example>
        public static RArray<T> Shuffle<T>(this IEnumerable<T> a)
#if NET10_0_OR_GREATER
            => Enumerable.Shuffle(a).ToRArray();
#else
        {
            var result = a.ToRArray();

            for (int i = result.Length - 1; i > 0; --i)
            {
                var j = Types.Random.Range(0, i + 1);
                (result[j], result[i]) = (result[i], result[j]);
            }

            return result;
        }
#endif

        /// <summary>
        /// Retourne l'élément de la séquence ayant la valeur minimale
        /// selon la clé numérique extraite par <paramref name="fn"/>.
        /// </summary>
        /// <typeparam name="T">Le type des éléments de la séquence.</typeparam>
        /// <param name="values">La séquence source.</param>
        /// <param name="fn">Fonction qui extrait la clé numérique (<see cref="long"/>) depuis un élément.</param>
        /// <returns>
        /// Un <see cref="MayBe{T}"/> contenant l'élément avec la clé minimale,
        /// ou <see cref="MayBe{T}.Null"/> si la séquence est vide.
        /// </returns>
        /// <example>
        /// <code>
        /// var people = new[] { new Person("Alice", 30), new Person("Bob", 25) };
        /// MayBe&lt;Person&gt; youngest = people.MinBy(p => p.Age);
        /// // youngest.Value => { "Bob", 25 }
        /// </code>
        /// </example>
        public static MayBe<T> MinBy<T>(this IEnumerable<T> values, Func<T, long> fn)
#if NETSTANDARD2_0 || NETSTANDARD2_1
        {
            MayBe<T> value = MayBe<T>.Null;
            MayBe<long> last = MayBe<long>.Null;

            foreach (var val in values)
            {
                var tmp = fn(val);

                if (last.IsEmpty || tmp < last)
                {
                    last = tmp;
                    value = val;
                }
            }

            return value;
        }
#else
        => values.Any() ? Enumerable.MinBy(values, fn) : MayBe<T>.Null;
#endif

        /// <summary>
        /// Retourne l'élément de la séquence ayant la valeur maximale
        /// selon la clé numérique extraite par <paramref name="fn"/>.
        /// </summary>
        /// <typeparam name="T">Le type des éléments de la séquence.</typeparam>
        /// <param name="values">La séquence source.</param>
        /// <param name="fn">Fonction qui extrait la clé numérique (<see cref="long"/>) depuis un élément.</param>
        /// <returns>
        /// Un <see cref="MayBe{T}"/> contenant l'élément avec la clé maximale,
        /// ou <see cref="MayBe{T}.Null"/> si la séquence est vide.
        /// </returns>
        /// <example>
        /// <code>
        /// var people = new[] { new Person("Alice", 30), new Person("Bob", 25) };
        /// MayBe&lt;Person&gt; oldest = people.MaxBy(p => p.Age);
        /// // oldest.Value => { "Alice", 30 }
        /// </code>
        /// </example>
        public static MayBe<T> MaxBy<T>(this IEnumerable<T> values, Func<T, long> fn)
#if NETSTANDARD2_0 || NETSTANDARD2_1
        {
            MayBe<T> value = MayBe<T>.Null;
            MayBe<long> last = MayBe<long>.Null;

            foreach (var val in values)
            {
                var tmp = fn(val);

                if (last.IsEmpty || tmp > last)
                {
                    last = tmp;
                    value = val;
                }
            }

            return value;
        }
#else
        => values.Any() ?  Enumerable.MaxBy(values, fn) : MayBe<T>.Null;
#endif

    }
}