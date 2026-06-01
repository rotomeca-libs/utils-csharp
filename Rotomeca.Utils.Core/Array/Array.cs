using System.Security.Cryptography.X509Certificates;

namespace Rotomeca.Utils.Array
{
    /// <summary>
    /// Fournit des utilitaires tableaux inspirés des API JavaScript,
    /// portés pour un usage idiomatique en C#.
    /// </summary>
    public static partial class Array
    {
        /// <summary>
        /// Divise un tableau en plusieurs sous-tableaux (chunks) d'une taille donnée.
        /// Le dernier chunk peut être plus petit si la taille du tableau n'est pas
        /// un multiple de <paramref name="size"/>.
        /// </summary>
        /// <typeparam name="T">Le type des éléments du tableau.</typeparam>
        /// <param name="array">Le tableau source à découper.</param>
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
        public static T[][] Chunck<T>(T[] array, uint size)
        {
            if (size == 0) return [];

#if NET5_0_OR_GREATER
            return [.. array.Chunk((int)size)];
#else
            var length = array.Length;

            // Calcule le nombre de chunks nécessaires (arrondi à l'entier supérieur)
            var chunkCount = Math.Ceiling(length / (double)size);
            var result = new T[(int)chunkCount][];

            var resultIndex = 0;
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

                result[resultIndex++] = subArray;
                arrIndex += (int)size;
            }

            return result;
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
        public static T[] Unique<T>(T[] array) => [.. array.Distinct()];

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
        /// <param name="array">Le tableau source.</param>
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
        public static T[] UniqueBy<T, TResult>(T[] array, Func<T, TResult> fn) where TResult : notnull
        {
#if NET5_0_OR_GREATER
            return [.. array.DistinctBy(fn)];
#else
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

            return [.. result];
#endif
        }
    }
}