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
        /// int[] source = [1, 2, 3, 4, 5];
        /// int[][] chunks = Array.Chunck(source, 2);
        /// // chunks => [[1, 2], [3, 4], [5]]
        /// </code>
        /// </example>
        public static RArray<RArray<T>> Chunk<T>(this IEnumerable<T> original, uint size)
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
        /// int[] unique = Array.Unique(source);
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
        /// Person[] unique = Array.UniqueBy(people, p => p.Name);
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

        public static Dictionary<TKey, RArray<T>> GroupTo<TKey, T>(this IEnumerable<T> original, Func<T, TKey> fn) where TKey : notnull
        {
            Dictionary<TKey, RArray<T>> result = [];

            foreach (var item in original.GroupBy(fn))
            {
                result.Add(item.Key, item.ToRArray());
            }

            return result;
        }

        public static MayBe<T> First<T>(this IEnumerable<T> values)
        {
            if (values.Any()) return Enumerable.First(values);
            else return MayBe<T>.Null;
        }

        public static MayBe<T> Last<T>(this IEnumerable<T> values)
        {
            if (values.Any()) return Enumerable.Last(values);
            else return MayBe<T>.Null;
        }

#if NET7_0_OR_GREATER
        public static T Sum<T>(this IEnumerable<T> values) where T : INumber<T>
            => values.Aggregate(T.Zero, (a, b) => a + b);
#else
        public static T Sum<T>(this IEnumerable<T> values) where T : Interfaces.IAggregable<T> => values.Aggregate(default(T)!, (a, b) => a.Add(b));
        public static int Sum(this IEnumerable<int> values) => values.Aggregate(0, (a, b) => a + b);
        public static long Sum(this IEnumerable<long> values) => values.Aggregate(0L, (a, b) => a + b);
        public static float Sum(this IEnumerable<float> values) => values.Aggregate(0f, (a, b) => a + b);
        public static double Sum(this IEnumerable<double> values) => values.Aggregate(0.0, (a, b) => a + b);
        public static decimal Sum(this IEnumerable<decimal> values) => values.Aggregate(0m, (a, b) => a + b);
#endif

        public static RArray<T> SortBy<T, TSort>(this IEnumerable<T> value, Func<T, TSort> fn) => value.OrderBy(fn).ToRArray();

        public static RArray<T> Flatten<T>(this IEnumerable<IEnumerable<T>> value) => value.SelectMany(x => x).ToRArray();

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

        public static RArray<T> Compact<T>(this IEnumerable<T> values) where T : class
            => values.Where(v => v != null).ToRArray();

        public static RArray<T> Compact<T>(this IEnumerable<MayBe<T>> values)
            => values.Where(v => v.HasValue).Select(x => x.Value!).ToRArray();

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

        public static Types.JsObject PartitionToObject<T>(this IEnumerable<T> values, Func<T, bool> predicate)
        {
            var (truthy, falsy) = values.Partition(predicate);
            return new Types.JsObject
            {
                ["truthy"] = truthy,
                ["falsy"] = falsy
            };
        }

        public static RArray<T> Intersection<T>(this IEnumerable<T> values, IEnumerable<T> others) => values.Intersect(others).ToRArray();

        public static RArray<T> Difference<T>(this IEnumerable<T> values, IEnumerable<T> others) => values.Except(others).ToRArray();

        public static RArray<T> Union<T>(IEnumerable<T> values, IEnumerable<T> others) => values.Union(others).Unique();

#if NETSTANDARD2_0 || NETSTANDARD2_1
        public static RArray<(T, Y)> Zip<T, Y>(this IEnumerable<T> a, IEnumerable<Y> b)
            => Enumerable.Zip(a, b, (x, y) => (x, y)).ToRArray();
#else
        public static RArray<(T, Y)> Zip<T, Y>(this IEnumerable<T> a, IEnumerable<Y> b)
            => Enumerable.Zip(a, b).ToRArray();
#endif

        public static RArray<T> Take<T>(this IEnumerable<T> a, uint nb) => Enumerable.Take(a, (int)nb).ToRArray();

        public static RArray<T> Drop<T>(this IEnumerable<T> a, uint nb) => a.Skip((int)nb).ToRArray();

        public static RArray<T> Shuffle<T>(this IEnumerable<T> a)
#if NET10_0_OR_GREATER
            => Enumerable.Shuffle(a).ToRArray();
#else
        {
            var result = a.ToRArray();

            var random = new System.Random();
            for (int i = result.Length - 1; i > 0; --i)
            {
                var j = random.Next(0, i + 1);
                (result[j], result[i]) = (result[i], result[j]);
            }

            return result;
        }
#endif

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
        => Enumerable.MinBy<T, long>(values, fn);
#endif

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
        => Enumerable.MaxBy<T, long>(values, fn);
#endif

    }
}