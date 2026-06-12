using Rotomeca.Core.Optionals;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Rotomeca.Utils.Dictionaries
{
    public static class Dicts
    {
        public static IDictionary<TKey, TValue> Pick<TKey, TValue>(
            this IDictionary<TKey, TValue> obj,
            params TKey[] keys) where TKey : notnull
        {
            if (keys.Length == 0) return new Dictionary<TKey, TValue>();

            var keyLength = keys.Length;
            Dictionary<TKey, TValue> result = new(keyLength);

            for (int i = 0; i < keyLength; ++i)
            {
                TKey key = keys[i];
                if (obj.TryGetValue(key, out TValue? value))
                    result.Add(key, value!);
            }

            return result;
        }

        public static IDictionary<TKey, TValue> Omit<TKey, TValue>(
            this IDictionary<TKey, TValue> obj,
            params TKey[] keys) where TKey : notnull
        {
            if (keys.Length == 0) return new Dictionary<TKey, TValue>(obj);
            var keyLength = keys.Length;
            Dictionary<TKey, TValue> result = new(obj);
            for (int i = 0; i < keyLength; ++i)
            {
                TKey key = keys[i];
                result.Remove(key);
            }
            return result;
        }


#if !NET5_0_OR_GREATER
        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
#endif

        private static object? CloneInternal(object? obj, Dictionary<object, object> visited)
        {
            if (obj is null) return null;

            var type = obj.GetType();

            // Immuables — on partage la référence sans risque
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
                return obj;

            // Référence circulaire
            if (visited.TryGetValue(obj, out var cached))
                return cached;

            // Tableaux
            if (type.IsArray)
            {
                var source = (Array)obj;
                var clone = Array.CreateInstance(type.GetElementType()!, source.Length);
                visited[obj] = clone;
                for (var i = 0; i < source.Length; i++)
                    clone.SetValue(CloneInternal(source.GetValue(i), visited), i);
                return clone;
            }

            // Tout autre objet : on crée une instance sans appeler le constructeur
#if NETSTANDARD2_0
            var result = FormatterServices.GetUninitializedObject(type);
#else
            var result = RuntimeHelpers.GetUninitializedObject(type);
#endif
            visited[obj] = result;

            var current = type;
            while (current is not null)
            {
                foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    field.SetValue(result, CloneInternal(field.GetValue(obj), visited));

                current = current.BaseType;
            }

            return result;
        }


#if NET5_0_OR_GREATER
        private static T JsonClone<T>(T obj, Action<string>? logger = null)
        {
            logger ??= (msg) => Console.WriteLine(msg);
            logger("DeepClone: réflexion impossible, fallback JSON (pertes possibles)");
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json)!;
        }
#endif

        public static T DeepClone<T>(this T obj, Action<string>? logger = null)
        {
#if NET5_0_OR_GREATER
            logger ??= msg => Console.Error.WriteLine(msg);
            try
            {
                return (T)CloneInternal(obj, new Dictionary<object, object>(ReferenceEqualityComparer.Instance))!;
            }
            catch
            {
                return JsonClone(obj, logger);
            }
#else
            logger ??= msg => Console.Error.WriteLine(msg);
            return (T)CloneInternal(obj, new Dictionary<object, object>(ReferenceComparer.Instance))!;
#endif
        }
    }
}