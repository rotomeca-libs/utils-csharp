using Rotomeca.Utils.Array.Interfaces;
using Rotomeca.Utils.Async.Helpers;
using Rotomeca.Utils.Types;
using Rotomeca.Utils.Types.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rotomeca.Utils.Array
{
    public class RArray<T> : IRArray<T>
#if NET7_0_OR_GREATER
        , IRArrayFactory<T, RArray<T>>
#endif
    {
        private List<T> _values;

        public RArray()
        {
            _values = [];
        }

        public RArray(params T[] values)
        {
            _values = [.. values];
        }

        public RArray(IEnumerable<T> values)
        {
            _values = [.. values];
        }


        public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Length => _values.Count;

        public static IRArray<T> From(IEnumerable<T> source)
        {
            return new RArray<T>(source);
        }

        public static async Task<RArray<T>> FromAsync(IAsyncEnumerable<T> source)
        {
            var result = new RArray<T>();

            await foreach (var item in source)
            {
                result.Push(item);
            }

            return result;
        }

        public static IRArray<T> Of(params T[] items)
        {
            return From(items);
        }

        public T At(int index) => index >= 0 ? _AtPos((uint)index) : _AtNeg(index);

        private T _AtPos(uint index)
        {
            if (index >= _values.Count) throw new IndexOutOfRangeException($"Index {index} is out of range for array of length {_values.Count}.");

            return _values[(int)index];
        }

        private T _AtNeg(Internal.NInt index)
        {
            var realIndex = _values.Count + index;

            if (realIndex < 0) throw new IndexOutOfRangeException($"Index {realIndex} from {index} is out of range for array of length {_values.Count}.");

            return _AtPos((uint)realIndex);
        }

        public IRArray<T> Concat(params IEnumerable<T>[] others)
        {
            List<T> tmp = [.. _values];

            foreach (var other in others)
            {
                tmp.AddRange(other);
            }


            return new RArray<T>(tmp);
        }

        public IRArray<T> CopyWithin(int target, int start = 0, int? end = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(int Index, T Value)> Entries()
        {
            return _values.Select((v, i) => (i, v));
        }

        public bool Every(Func<T, bool> fn)
        {
            return _values.All(fn);
        }

        public IRArray<T> Fill(T value, int start = 0, int? end = null)
        {
            return new RArray<T>(Enumerable.Range(0, Length).Select(i => (i >= start && i < (end ?? Length)) ? value : _values[i]));
        }

        public IRArray<T> Filter(Func<T, bool> fn)
        {
            return new RArray<T>(_values.Where(fn));
        }

        public T? Find(Func<T, bool> fn)
        {
            return _values.FirstOrDefault(fn);
        }

        public int FindIndex(Func<T, bool> fn) => _values.FindIndex(x => fn(x));

        public T? FindLast(Func<T, bool> fn) => _values.FindLast(x => fn(x));
        public int FindLastIndex(Func<T, bool> fn) => _values.FindLastIndex(x => fn(x));

        public IRArray<TResult> FlatMap<TResult>(Func<T, IEnumerable<TResult>> fn)
        {
            return new RArray<TResult>(_values.SelectMany(fn));
        }

        public void ForEach(Action<T> fn)
        {
            foreach (var item in _values)
            {
                fn(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public bool Includes(T value)
        {
            return _values.Contains(value);
        }

        public int IndexOf(T value)
        {
            return _values.IndexOf(value);
        }

        public string Join(string separator = ",")
        {
            return string.Join(separator, _values);
        }

        public IEnumerable<int> Keys()
        {
            return Enumerable.Range(0, Length);
        }

        public int LastIndexOf(T value)
        {
            throw new NotImplementedException();
        }

        public IRArray<TResult> Map<TResult>(Func<T, TResult> fn)
        {
            throw new NotImplementedException();
        }

        public T? Pop()
        {
            throw new NotImplementedException();
        }

        public int Push(params T[] items)
        {
            throw new NotImplementedException();
        }

        public TResult Reduce<TResult>(Func<TResult, T, TResult> fn, TResult initialValue)
        {
            throw new NotImplementedException();
        }

        public TResult ReduceRight<TResult>(Func<TResult, T, TResult> fn, TResult initialValue)
        {
            throw new NotImplementedException();
        }

        public IRArray<T> Reverse()
        {
            throw new NotImplementedException();
        }

        public T? Shift()
        {
            throw new NotImplementedException();
        }

        public IRArray<T> Slice(int start = 0, int? end = null)
        {
            throw new NotImplementedException();
        }

        public bool Some(Func<T, bool> fn)
        {
            throw new NotImplementedException();
        }

        public IRArray<T> Sort(Comparison<T>? compareFn = null)
        {
            throw new NotImplementedException();
        }

        public IRArray<T> Splice(int start, int? deleteCount = null, params T[] items)
        {
            throw new NotImplementedException();
        }

        public T[] ToArray()
        {
            throw new NotImplementedException();
        }

        public IRArray<T> ToReversed()
        {
            throw new NotImplementedException();
        }

        public IRArray<T> ToSorted(Comparison<T>? compareFn = null)
        {
            throw new NotImplementedException();
        }

        public IRArray<T> ToSpliced(int start, int? deleteCount = null, params T[] items)
        {
            throw new NotImplementedException();
        }

        public int Unshift(params T[] items)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Values()
        {
            throw new NotImplementedException();
        }

        public IRArray<T> With(int index, T value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    namespace Internal
    {
        internal readonly struct NInt(int value = 0) : IClampedValue<int>
        {
            private readonly ClampedValue<int> _internal = new(int.MinValue, 0, value);

            /// <inheritdoc/>
            public int Min => _internal.Min;

            /// <inheritdoc/>
            public int Max => _internal.Max;

            /// <inheritdoc/>
            public int Value => _internal.Value;

            public ClampedValue<int> WithValue(int value) => new(int.MinValue, 0, value);

            /// <inheritdoc/>
            public override string ToString() => Value.ToString();

            public static implicit operator int(NInt nb) => nb.Value;

            public static implicit operator NInt(int nb) => new(nb);
        }
    }
}
