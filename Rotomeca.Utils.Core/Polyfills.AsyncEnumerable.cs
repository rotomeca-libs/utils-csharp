// Polyfills pour IAsyncEnumerable<T> sur netstandard2.0
// Aucune dépendance externe — implémentation minimale backed by Task
#if NETSTANDARD2_0

// ── ValueTask / ValueTask<T> ─────────────────────────────────────────────────

namespace System.Threading.Tasks
{
    /// <summary>Polyfill minimal de ValueTask pour netstandard2.0.</summary>
    internal readonly struct ValueTask : IValueTask
    {
        private readonly Task? _task;

        public ValueTask(Task task) => _task = task;

        public Runtime.CompilerServices.TaskAwaiter GetAwaiter()
            => (_task ?? Task.CompletedTask).GetAwaiter();

        public static ValueTask CompletedTask => default;
    }

    /// <summary>Polyfill minimal de ValueTask&lt;T&gt; pour netstandard2.0.</summary>
    internal readonly struct ValueTask<T> : IValueTask<T>
    {
        private readonly Task<T>? _task;
        private readonly T _value;

        public ValueTask(T value) { _value = value; _task = null; }
        public ValueTask(Task<T> task) { _task = task; _value = default!; }

        public Runtime.CompilerServices.TaskAwaiter<T> GetAwaiter()
            => (_task ?? Task.FromResult(_value)).GetAwaiter();
    }

    public interface IValueTask
    {
        Runtime.CompilerServices.TaskAwaiter GetAwaiter();
    }

    public interface IValueTask<T>
    {
        Runtime.CompilerServices.TaskAwaiter<T> GetAwaiter();
    }
}

// ── Interfaces ───────────────────────────────────────────────────────────────

namespace System
{
    /// <summary>Polyfill de IAsyncDisposable pour netstandard2.0.</summary>
    public interface IAsyncDisposable
    {
        Threading.Tasks.IValueTask DisposeAsync();
    }
}

namespace System.Collections.Generic
{
    /// <summary>Polyfill de IAsyncEnumerable&lt;T&gt; pour netstandard2.0.</summary>
    public interface IAsyncEnumerable<out T>
    {
        IAsyncEnumerator<T> GetAsyncEnumerator(
            Threading.CancellationToken cancellationToken = default);
    }

    /// <summary>Polyfill de IAsyncEnumerator&lt;T&gt; pour netstandard2.0.</summary>
    public interface IAsyncEnumerator<out T> : IAsyncDisposable
    {
        Threading.Tasks.IValueTask<bool> MoveNextAsync();
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
    internal sealed class AsyncEnumerable<T> : System.Collections.Generic.IAsyncEnumerable<T>
    {
        private readonly IEnumerable<Func<Task<T>>> _fns;

        public AsyncEnumerable(IEnumerable<Func<Task<T>>> fns) => _fns = fns;

        public System.Collections.Generic.IAsyncEnumerator<T> GetAsyncEnumerator(
            System.Threading.CancellationToken cancellationToken = default)
            => new AsyncEnumerator<T>(_fns.GetEnumerator(), cancellationToken);
    }

    internal sealed class AsyncEnumerator<T> : System.Collections.Generic.IAsyncEnumerator<T>
    {
        private readonly IEnumerator<Func<Task<T>>> _inner;
        private readonly System.Threading.CancellationToken _ct;

        public T Current { get; private set; } = default!;

        public AsyncEnumerator(
            IEnumerator<Func<Task<T>>> inner,
            System.Threading.CancellationToken ct)
        {
            _inner = inner;
            _ct = ct;
        }

        public System.Threading.Tasks.IValueTask<bool> MoveNextAsync()
        {
            if (_ct.IsCancellationRequested || !_inner.MoveNext())
                return new ValueTask<bool>(false);

            return new ValueTask<bool>(_MoveNextAsync(_inner.Current));
        }

        private async Task<bool> _MoveNextAsync(Func<Task<T>> fn)
        {
            Current = await fn();
            return true;
        }

        /// <summary>
        /// Sera supprimé, pour test
        /// </summary>
        /// <returns></returns>
        public async Task Test()
        {
            var tmp = new AsyncEnumerable<int>([async () => 0, async () => 1, async () => 2]);
            await foreach (var item in tmp)
            {

            }
        }

        public System.Threading.Tasks.IValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }
}

#endif