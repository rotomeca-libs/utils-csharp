// Polyfills pour IAsyncEnumerable<T> sur netstandard2.0
// Aucune dépendance externe — implémentation minimale backed by Task
#if NETSTANDARD2_0

// ── ValueTask / ValueTask<T> ─────────────────────────────────────────────────

namespace System.Threading.Tasks
{
    /// <summary>
    /// Polyfill minimal de <c>ValueTask</c> pour netstandard2.0.
    /// Encapsule une <see cref="Task"/> ou représente une complétion synchrone.
    /// </summary>
    internal readonly struct ValueTask : IValueTask
    {
        private readonly Task? _task;

        /// <summary>
        /// Initialise un <see cref="ValueTask"/> à partir d'une <see cref="Task"/> existante.
        /// </summary>
        /// <param name="task">La tâche sous-jacente.</param>
        public ValueTask(Task task) => _task = task;

        /// <inheritdoc/>
        public Runtime.CompilerServices.TaskAwaiter GetAwaiter()
            => (_task ?? Task.CompletedTask).GetAwaiter();

        /// <summary>
        /// Représente un <see cref="ValueTask"/> déjà complété avec succès.
        /// </summary>
        public static ValueTask CompletedTask => default;
    }

    /// <summary>
    /// Polyfill minimal de <c>ValueTask&lt;T&gt;</c> pour netstandard2.0.
    /// Encapsule soit une <see cref="Task{T}"/> asynchrone, soit une valeur déjà disponible.
    /// </summary>
    /// <typeparam name="T">Le type du résultat.</typeparam>
    internal readonly struct ValueTask<T> : IValueTask<T>
    {
        private readonly Task<T>? _task;
        private readonly T _value;

        /// <summary>
        /// Initialise un <see cref="ValueTask{T}"/> à partir d'une valeur déjà disponible
        /// (chemin synchrone, sans allocation de <see cref="Task{T}"/>).
        /// </summary>
        /// <param name="value">La valeur résultante.</param>
        public ValueTask(T value) { _value = value; _task = null; }

        /// <summary>
        /// Initialise un <see cref="ValueTask{T}"/> à partir d'une <see cref="Task{T}"/> asynchrone.
        /// </summary>
        /// <param name="task">La tâche sous-jacente.</param>
        public ValueTask(Task<T> task) { _task = task; _value = default!; }

        /// <inheritdoc/>
        public Runtime.CompilerServices.TaskAwaiter<T> GetAwaiter()
            => (_task ?? Task.FromResult(_value)).GetAwaiter();
    }

    /// <summary>
    /// Contrat minimal permettant d'awaiter un <see cref="ValueTask"/> sans résultat
    /// sur netstandard2.0.
    /// </summary>
    public interface IValueTask
    {
        /// <summary>Retourne l'awaiter de la tâche.</summary>
        Runtime.CompilerServices.TaskAwaiter GetAwaiter();
    }

    /// <summary>
    /// Contrat minimal permettant d'awaiter un <see cref="ValueTask{T}"/> avec résultat
    /// sur netstandard2.0.
    /// </summary>
    /// <typeparam name="T">Le type du résultat.</typeparam>
    public interface IValueTask<T>
    {
        /// <summary>Retourne l'awaiter de la tâche.</summary>
        Runtime.CompilerServices.TaskAwaiter<T> GetAwaiter();
    }
}

// ── Interfaces ───────────────────────────────────────────────────────────────

namespace System
{
    /// <summary>
    /// Polyfill de <c>IAsyncDisposable</c> pour netstandard2.0.
    /// Permet la libération asynchrone des ressources via <c>await using</c>.
    /// </summary>
    public interface IAsyncDisposable
    {
        /// <summary>
        /// Libère les ressources de manière asynchrone.
        /// </summary>
        /// <returns>
        /// Un <see cref="Threading.Tasks.IValueTask"/> représentant l'opération de libération.
        /// </returns>
        Threading.Tasks.IValueTask DisposeAsync();
    }
}

namespace System.Collections.Generic
{
    /// <summary>
    /// Polyfill de <c>IAsyncEnumerable&lt;T&gt;</c> pour netstandard2.0.
    /// Expose un flux de données asynchrone consommable via <c>await foreach</c>.
    /// </summary>
    /// <typeparam name="T">Le type des éléments du flux.</typeparam>
    public interface IAsyncEnumerable<out T>
    {
        /// <summary>
        /// Retourne un énumérateur asynchrone permettant d'itérer le flux.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token optionnel permettant d'annuler l'itération.
        /// </param>
        /// <returns>Un <see cref="IAsyncEnumerator{T}"/> pour parcourir le flux.</returns>
        IAsyncEnumerator<T> GetAsyncEnumerator(
            Threading.CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Polyfill de <c>IAsyncEnumerator&lt;T&gt;</c> pour netstandard2.0.
    /// Permet l'avancement pas-à-pas dans un flux asynchrone.
    /// </summary>
    /// <typeparam name="T">Le type des éléments du flux.</typeparam>
    public interface IAsyncEnumerator<out T> : IAsyncDisposable
    {
        /// <summary>
        /// Avance l'énumérateur jusqu'à l'élément suivant du flux.
        /// </summary>
        /// <returns>
        /// Un <see cref="Threading.Tasks.IValueTask{T}"/> qui vaut <c>true</c>
        /// si un élément est disponible, <c>false</c> si le flux est épuisé ou annulé.
        /// </returns>
        Threading.Tasks.IValueTask<bool> MoveNextAsync();

        /// <summary>Élément courant du flux.</summary>
        T Current { get; }
    }
}

// ── AsyncEnumerable<T> interne ───────────────────────────────────────────────

namespace Rotomeca.Utils.Async.Internal
{
    /// <summary>
    /// Implémentation interne de <see cref="System.Collections.Generic.IAsyncEnumerable{T}"/>
    /// pour netstandard2.0.
    /// </summary>
    /// <remarks>
    /// Remplace les state machines générées par le compilateur sur les targets
    /// qui supportent <c>async IAsyncEnumerable&lt;T&gt;</c> nativement.
    /// Backed by <see cref="System.Threading.Tasks.Task"/> — pas de <c>ValueTask</c>
    /// optimisé, mais fonctionnel sans dépendance externe.
    /// </remarks>
    /// <typeparam name="T">Le type des éléments produits par le flux.</typeparam>
    internal sealed class AsyncEnumerable<T> : System.Collections.Generic.IAsyncEnumerable<T>
    {
        private readonly IEnumerable<Func<Task<T>>> _fns;

        /// <summary>
        /// Initialise le flux à partir d'une séquence de fonctions asynchrones à exécuter dans l'ordre.
        /// </summary>
        /// <param name="fns">Les fonctions asynchrones constituant le flux.</param>
        public AsyncEnumerable(IEnumerable<Func<Task<T>>> fns) => _fns = fns;

        /// <inheritdoc/>
        public System.Collections.Generic.IAsyncEnumerator<T> GetAsyncEnumerator(
            System.Threading.CancellationToken cancellationToken = default)
            => new AsyncEnumerator<T>(_fns.GetEnumerator(), cancellationToken);
    }

    /// <summary>
    /// Énumérateur asynchrone interne pour netstandard2.0.
    /// Exécute séquentiellement chaque fonction du flux et expose son résultat via <see cref="Current"/>.
    /// </summary>
    /// <typeparam name="T">Le type des éléments produits.</typeparam>
    internal sealed class AsyncEnumerator<T> : System.Collections.Generic.IAsyncEnumerator<T>
    {
        private readonly IEnumerator<Func<Task<T>>> _inner;
        private readonly System.Threading.CancellationToken _ct;

        /// <inheritdoc/>
        public T Current { get; private set; } = default!;

        /// <summary>
        /// Initialise l'énumérateur à partir d'un itérateur de fonctions asynchrones
        /// et d'un token d'annulation.
        /// </summary>
        /// <param name="inner">L'itérateur sur les fonctions asynchrones à exécuter.</param>
        /// <param name="ct">Token permettant d'interrompre l'itération.</param>
        public AsyncEnumerator(
            IEnumerator<Func<Task<T>>> inner,
            System.Threading.CancellationToken ct)
        {
            _inner = inner;
            _ct = ct;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Retourne immédiatement <c>false</c> si le token est annulé ou si le flux est épuisé.
        /// Sinon, exécute la prochaine fonction et stocke son résultat dans <see cref="Current"/>.
        /// </remarks>
        public System.Threading.Tasks.IValueTask<bool> MoveNextAsync()
        {
            if (_ct.IsCancellationRequested || !_inner.MoveNext())
                return new ValueTask<bool>(false);

            return new ValueTask<bool>(_MoveNextAsync(_inner.Current));
        }

        /// <summary>
        /// Exécute la fonction asynchrone courante et met à jour <see cref="Current"/>.
        /// </summary>
        /// <param name="fn">La fonction à exécuter.</param>
        /// <returns><c>true</c> une fois la valeur disponible.</returns>
        private async Task<bool> _MoveNextAsync(Func<Task<T>> fn)
        {
            Current = await fn();
            return true;
        }

        /// <inheritdoc/>
        /// <remarks>Libère l'itérateur interne de manière synchrone.</remarks>
        public System.Threading.Tasks.IValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }
}

#endif