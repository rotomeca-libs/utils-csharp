using Rotomeca.Utils.Types;
using System.Collections.Concurrent;

namespace Rotomeca.Utils.Async
{
    /// <summary>
    /// Fournit des utilitaires asynchrones inspirés des API JavaScript,
    /// portés pour un usage idiomatique en C#.
    /// </summary>
    public static partial class Async
    {
        public static Task<T[]> Parallel<T>(params Func<Task<T>>[] tasks) => Task.WhenAll(tasks.Select(fn => fn()));
    }
}
