namespace Rotomeca.Utils.Array.Interfaces
{
    /// <summary>
    /// Contrat de base des collections Rotomeca, inspiré de l'API <c>Array</c> JavaScript.
    /// </summary>
    /// <remarks>
    /// Les méthodes sont organisées en catégories correspondant à la documentation MDN :
    /// accès, mutation, recherche, test, transformation, itération et conversion.
    /// <para>
    /// Les méthodes <c>To*</c> (ToReversed, ToSorted, ToSpliced, With) sont les
    /// équivalents non-mutants introduits en ES2023.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">Le type des éléments.</typeparam>
    public interface IRArray<T> : IEnumerable<T>
    {
        /// <summary>
        /// Accesseur indexé pour lire ou modifier l'élément à une position donnée.
        /// </summary>
        /// <param name="index">Index de l'élément</param>
        /// <returns>Elément trouver</returns>
        /// <exception cref="IndexOutOfRangeException">Si l'index n'existe pas</exception>
        T this[int index] { get; set; }

        // ── Propriétés ─────────────────────────────────────────────────────────

        /// <summary>Nombre d'éléments dans la collection.</summary>
        int Length { get; }

        // ── Accès ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne l'élément à l'index donné.
        /// Supporte les index négatifs : <c>-1</c> correspond au dernier élément.
        /// </summary>
        /// <param name="index">Index positif ou négatif.</param>
        T At(int index);

        // ── Mutation ───────────────────────────────────────────────────────────

        /// <summary>
        /// Ajoute un ou plusieurs éléments en fin de collection.
        /// </summary>
        /// <param name="items">Éléments à ajouter.</param>
        /// <returns>La nouvelle taille de la collection.</returns>
        int Push(params T[] items);

        /// <summary>Supprime et retourne le dernier élément.</summary>
        T? Pop();

        /// <summary>
        /// Ajoute un ou plusieurs éléments en début de collection.
        /// </summary>
        /// <param name="items">Éléments à ajouter.</param>
        /// <returns>La nouvelle taille de la collection.</returns>
        int Unshift(params T[] items);

        /// <summary>Supprime et retourne le premier élément.</summary>
        T? Shift();

        /// <summary>
        /// Modifie la collection en supprimant et/ou insérant des éléments à partir de <paramref name="start"/>.
        /// </summary>
        /// <param name="start">Index de départ (supporte les négatifs).</param>
        /// <param name="deleteCount">Nombre d'éléments à supprimer. Si <c>null</c>, supprime jusqu'à la fin.</param>
        /// <param name="items">Éléments à insérer à la place des éléments supprimés.</param>
        /// <returns>Un nouveau tableau contenant les éléments supprimés.</returns>
        IRArray<T> Splice(int start, int? deleteCount = null, params T[] items);

        /// <summary>
        /// Remplace tous les éléments entre <paramref name="start"/> et <paramref name="end"/> par <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Valeur de remplacement.</param>
        /// <param name="start">Index de début (inclus). Défaut : 0.</param>
        /// <param name="end">Index de fin (exclus). Défaut : fin du tableau.</param>
        /// <returns>La collection modifiée (mutation en place).</returns>
        IRArray<T> Fill(T value, int start = 0, int? end = null);

        /// <summary>
        /// Copie une portion du tableau vers la position <paramref name="target"/> (mutation en place).
        /// </summary>
        /// <param name="target">Index de destination.</param>
        /// <param name="start">Index source de début. Défaut : 0.</param>
        /// <param name="end">Index source de fin (exclus). Défaut : fin du tableau.</param>
        /// <returns>La collection modifiée.</returns>
        IRArray<T> CopyWithin(int target, int start = 0, int? end = null);

        /// <summary>Trie la collection en place.</summary>
        /// <param name="compareFn">Fonction de comparaison. Si <c>null</c>, utilise l'ordre naturel.</param>
        /// <returns>La collection modifiée.</returns>
        IRArray<T> Sort(Comparison<T>? compareFn = null);

        /// <summary>Inverse la collection en place.</summary>
        /// <returns>La collection modifiée.</returns>
        IRArray<T> Reverse();

        // ── Recherche ──────────────────────────────────────────────────────────

        /// <summary>Retourne le premier index de <paramref name="value"/>, ou <c>-1</c> si absent.</summary>
        int IndexOf(T value);

        /// <summary>Retourne le dernier index de <paramref name="value"/>, ou <c>-1</c> si absent.</summary>
        int LastIndexOf(T value);

        /// <summary>Retourne le premier élément satisfaisant <paramref name="fn"/>, ou la valeur par défaut.</summary>
        T? Find(Func<T, bool> fn);

        /// <summary>Retourne l'index du premier élément satisfaisant <paramref name="fn"/>, ou <c>-1</c>.</summary>
        int FindIndex(Func<T, bool> fn);

        /// <summary>Retourne le dernier élément satisfaisant <paramref name="fn"/>, ou la valeur par défaut.</summary>
        T? FindLast(Func<T, bool> fn);

        /// <summary>Retourne l'index du dernier élément satisfaisant <paramref name="fn"/>, ou <c>-1</c>.</summary>
        int FindLastIndex(Func<T, bool> fn);

        /// <summary>Retourne <c>true</c> si la collection contient <paramref name="value"/>.</summary>
        bool Includes(T value);

        // ── Test ───────────────────────────────────────────────────────────────

        /// <summary>Retourne <c>true</c> si tous les éléments satisfont <paramref name="fn"/>.</summary>
        bool Every(Func<T, bool> fn);

        /// <summary>Retourne <c>true</c> si au moins un élément satisfait <paramref name="fn"/>.</summary>
        bool Some(Func<T, bool> fn);

        // ── Transformation ─────────────────────────────────────────────────────

        /// <summary>Retourne un nouveau tableau ne contenant que les éléments satisfaisant <paramref name="fn"/>.</summary>
        IRArray<T> Filter(Func<T, bool> fn);

        /// <summary>Retourne un nouveau tableau avec chaque élément transformé par <paramref name="fn"/>.</summary>
        /// <typeparam name="TResult">Type de retour de la transformation.</typeparam>
        IRArray<TResult> Map<TResult>(Func<T, TResult> fn);

        /// <summary>
        /// Transforme chaque élément en une collection via <paramref name="fn"/>, puis aplatit d'un niveau.
        /// </summary>
        /// <typeparam name="TResult">Type des éléments après aplatissement.</typeparam>
        IRArray<TResult> FlatMap<TResult>(Func<T, IEnumerable<TResult>> fn);

        /// <summary>Réduit la collection à une valeur unique, de gauche à droite.</summary>
        /// <typeparam name="TResult">Type de l'accumulateur.</typeparam>
        /// <param name="fn">Fonction accumulatrice : (accumulateur, élément courant) => résultat.</param>
        /// <param name="initialValue">Valeur initiale de l'accumulateur.</param>
        TResult Reduce<TResult>(Func<TResult, T, TResult> fn, TResult initialValue);

        /// <summary>Réduit la collection à une valeur unique, de droite à gauche.</summary>
        /// <typeparam name="TResult">Type de l'accumulateur.</typeparam>
        /// <param name="fn">Fonction accumulatrice : (accumulateur, élément courant) => résultat.</param>
        /// <param name="initialValue">Valeur initiale de l'accumulateur.</param>
        TResult ReduceRight<TResult>(Func<TResult, T, TResult> fn, TResult initialValue);

        /// <summary>Fusionne cette collection avec une ou plusieurs autres. Non-mutant.</summary>
        IRArray<T> Concat(params IEnumerable<T>[] others);

        /// <summary>Extrait une portion de la collection. Non-mutant.</summary>
        /// <param name="start">Index de début (inclus, supporte les négatifs). Défaut : 0.</param>
        /// <param name="end">Index de fin (exclus, supporte les négatifs). Défaut : fin du tableau.</param>
        IRArray<T> Slice(int start = 0, int? end = null);

        // ── Copie non-mutante (ES2023+) ────────────────────────────────────────

        /// <summary>Retourne une copie inversée sans modifier l'original.</summary>
        IRArray<T> ToReversed();

        /// <summary>Retourne une copie triée sans modifier l'original.</summary>
        /// <param name="compareFn">Fonction de comparaison. Si <c>null</c>, utilise l'ordre naturel.</param>
        IRArray<T> ToSorted(Comparison<T>? compareFn = null);

        /// <summary>Retourne une copie avec splice appliqué, sans modifier l'original.</summary>
        /// <param name="start">Index de départ.</param>
        /// <param name="deleteCount">Nombre d'éléments à supprimer. Si <c>null</c>, supprime jusqu'à la fin.</param>
        /// <param name="items">Éléments à insérer.</param>
        IRArray<T> ToSpliced(int start, int? deleteCount = null, params T[] items);

        /// <summary>
        /// Retourne une copie avec l'élément à <paramref name="index"/> remplacé par <paramref name="value"/>.
        /// Non-mutant.
        /// </summary>
        IRArray<T> With(int index, T value);

        // ── Itération ──────────────────────────────────────────────────────────

        /// <summary>Exécute <paramref name="fn"/> pour chaque élément de la collection.</summary>
        void ForEach(Action<T> fn);

        /// <summary>Retourne un itérateur de paires <c>(index, valeur)</c>.</summary>
        IEnumerable<(int Index, T Value)> Entries();

        /// <summary>Retourne un itérateur des indices.</summary>
        IEnumerable<int> Keys();

        /// <summary>Retourne un itérateur des valeurs.</summary>
        IEnumerable<T> Values();

        // ── Conversion ─────────────────────────────────────────────────────────

        /// <summary>
        /// Joint tous les éléments en une chaîne de caractères.
        /// </summary>
        /// <param name="separator">Séparateur entre les éléments. Défaut : <c>","</c>.</param>
        string Join(string separator = ",");

        /// <summary>Retourne la collection sous forme de tableau natif <typeparamref name="T"/>[].</summary>
        T[] ToArray();

        // ── Méthodes statiques — C# 11 / .NET 7+ ──────────────────────────────

#if NET7_0_OR_GREATER
        /// <summary>
        /// Crée un <see cref="IRArray{T}"/> à partir d'un <see cref="IEnumerable{T}"/>.
        /// </summary>
        static abstract IRArray<T> From(IEnumerable<T> source);

        /// <summary>
        /// Crée un <see cref="IRArray{T}"/> à partir des éléments passés en argument.
        /// </summary>
        static abstract IRArray<T> Of(params T[] items);
#endif
    }


#if NET7_0_OR_GREATER
    /// <summary>
    /// Contrat des méthodes statiques de fabrique, inspiré de Array.from() / Array.of() en JS.
    /// </summary>
    /// <typeparam name="T">Le type des éléments.</typeparam>
    /// <typeparam name="TSelf">
    /// Le type concret qui implémente cette interface (pattern CRTP).
    /// Permet au compilateur de résoudre les membres statiques abstraits.
    /// </typeparam>
    /// <example>
    /// <code>
    /// public class RArray&lt;T&gt; : IRArray&lt;T&gt;, IRArrayFactory&lt;T, RArray&lt;T&gt;&gt;
    /// {
    ///     public static RArray&lt;T&gt; From(IEnumerable&lt;T&gt; source) => new([.. source]);
    /// }
    /// </code>
    /// </example>
    public interface IRArrayFactory<T, TSelf> where TSelf : IRArray<T>
    {
        /// <summary>Crée une instance à partir d'un <see cref="IAsyncEnumerable{T}"/> (ES2024).</summary>
        static abstract Task<TSelf> FromAsync(IAsyncEnumerable<T> source);
    }
#endif
}