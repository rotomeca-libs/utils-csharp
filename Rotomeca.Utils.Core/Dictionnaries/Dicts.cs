using Rotomeca.Core.Optionals;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
#if NET5_0_OR_GREATER
using System.Text.Json;
#endif


namespace Rotomeca.Utils.Dictionaries
{
    /// <summary>
    /// Fournit des méthodes d'extension utilitaires pour les dictionnaires.
    /// </summary>
    public static class Dicts
    {
        /// <summary>
        /// Retourne un nouveau dictionnaire contenant uniquement les clés spécifiées.
        /// Les clés absentes du dictionnaire source sont silencieusement ignorées.
        /// </summary>
        /// <typeparam name="TKey">Le type des clés.</typeparam>
        /// <typeparam name="TValue">Le type des valeurs.</typeparam>
        /// <param name="obj">Le dictionnaire source.</param>
        /// <param name="keys">Les clés à conserver.</param>
        /// <returns>
        /// Un nouveau dictionnaire contenant uniquement les entrées dont la clé figure dans <paramref name="keys"/>.
        /// Retourne un dictionnaire vide si <paramref name="keys"/> est vide.
        /// </returns>
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

        /// <summary>
        /// Retourne un nouveau dictionnaire excluant les clés spécifiées.
        /// Les clés absentes du dictionnaire source sont silencieusement ignorées.
        /// </summary>
        /// <typeparam name="TKey">Le type des clés.</typeparam>
        /// <typeparam name="TValue">Le type des valeurs.</typeparam>
        /// <param name="obj">Le dictionnaire source.</param>
        /// <param name="keys">Les clés à exclure.</param>
        /// <returns>
        /// Un nouveau dictionnaire contenant toutes les entrées sauf celles dont la clé figure dans <paramref name="keys"/>.
        /// Retourne une copie complète du dictionnaire source si <paramref name="keys"/> est vide.
        /// </returns>
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

        /// <summary>
        /// Effectue un clonage profond récursif d'un objet en parcourant ses champs via réflexion.
        /// Gère les références circulaires via un dictionnaire de visites.
        /// Les types immuables (primitifs, chaînes, énumérations) sont partagés par référence.
        /// </summary>
        /// <param name="obj">L'objet à cloner. Peut être <c>null</c>.</param>
        /// <param name="visited">
        /// Dictionnaire des objets déjà clonés, indexés par référence source.
        /// Permet d'éviter les boucles infinies en cas de graphe circulaire.
        /// </param>
        /// <returns>Un clone profond de <paramref name="obj"/>, ou <c>null</c> si l'entrée est <c>null</c>.</returns>
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
        /// <summary>
        /// Stratégie de secours pour <see cref="DeepClone{T}"/> lorsque la réflexion échoue.
        /// Sérialise l'objet en JSON puis le désérialise dans une nouvelle instance.
        /// </summary>
        /// <remarks>
        /// Cette approche peut entraîner des pertes de données pour les types non sérialisables
        /// (champs privés, types polymorphiques non annotés, etc.).
        /// </remarks>
        /// <typeparam name="T">Le type de l'objet à cloner.</typeparam>
        /// <param name="obj">L'objet à cloner.</param>
        /// <param name="logger">Action de journalisation. Par défaut : écriture sur <see cref="Console.WriteLine(string)"/>.</param>
        /// <returns>Un clone de <paramref name="obj"/> obtenu par round-trip JSON.</returns>
        private static T JsonClone<T>(T obj, Action<string>? logger = null)
        {
            logger ??= (msg) => Console.WriteLine(msg);
            logger("DeepClone: réflexion impossible, fallback JSON (pertes possibles)");
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json)!;
        }
#endif

        /// <summary>
        /// Effectue un clonage profond de l'objet.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Sur .NET 5+ : utilise la réflexion pour parcourir tous les champs (y compris privés et hérités),
        /// avec détection des références circulaires. En cas d'échec, bascule automatiquement
        /// sur un clonage JSON (voir <c>JsonClone</c>).
        /// </para>
        /// <para>
        /// Sur .NET Standard 2.0 : utilise uniquement la réflexion, sans fallback JSON.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">Le type de l'objet à cloner.</typeparam>
        /// <param name="obj">L'objet à cloner.</param>
        /// <param name="logger">
        /// Action de journalisation appelée en cas de fallback JSON.
        /// Par défaut : écriture sur <see cref="Console.Error"/>.
        /// </param>
        /// <returns>Un clone profond de <paramref name="obj"/>.</returns>
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

        /// <summary>
        /// Indique si le dictionnaire est vide.
        /// </summary>
        /// <typeparam name="TKey">Le type des clés.</typeparam>
        /// <typeparam name="TValue">Le type des valeurs.</typeparam>
        /// <param name="dict">Le dictionnaire à tester.</param>
        /// <returns><c>true</c> si le dictionnaire ne contient aucune entrée ; <c>false</c> sinon.</returns>
        public static bool IsEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dict) => dict.Count == 0;

        /// <summary>
        /// Retourne un nouveau dictionnaire dont les valeurs ont été transformées par la fonction <paramref name="fn"/>.
        /// Les clés sont conservées à l'identique.
        /// </summary>
        /// <typeparam name="TKey">Le type des clés.</typeparam>
        /// <typeparam name="TValue">Le type des valeurs sources.</typeparam>
        /// <typeparam name="TNewValue">Le type des valeurs résultantes.</typeparam>
        /// <param name="dict">Le dictionnaire source.</param>
        /// <param name="fn">
        /// Fonction de transformation appelée avec la valeur et la clé courantes.
        /// Retourne la nouvelle valeur associée à cette clé.
        /// </param>
        /// <returns>Un nouveau dictionnaire avec les mêmes clés et les valeurs transformées.</returns>
        public static IDictionary<TKey, TNewValue> MapValues<TKey, TValue, TNewValue>(this IDictionary<TKey, TValue> dict, Func<TValue, TKey, TNewValue> fn) where TKey : notnull
            => dict.Select(x =>
            {
                var mapped = fn(x.Value, x.Key);
                return (key: x.Key, value: mapped);
            }).ToDictionary(x => x.key, x => x.value);

        /// <summary>
        /// Retourne un nouveau dictionnaire dont les clés ont été transformées par la fonction <paramref name="fn"/>.
        /// Les valeurs sont conservées à l'identique.
        /// </summary>
        /// <remarks>
        /// Si <paramref name="fn"/> produit des clés dupliquées, une exception sera levée lors de la construction du dictionnaire résultant.
        /// </remarks>
        /// <typeparam name="TKey">Le type des clés sources.</typeparam>
        /// <typeparam name="TValue">Le type des valeurs.</typeparam>
        /// <typeparam name="TNewKey">Le type des clés résultantes.</typeparam>
        /// <param name="dict">Le dictionnaire source.</param>
        /// <param name="fn">
        /// Fonction de transformation appelée avec la clé et la valeur courantes.
        /// Retourne la nouvelle clé à utiliser pour cette entrée.
        /// </param>
        /// <returns>Un nouveau dictionnaire avec les clés transformées et les mêmes valeurs.</returns>
        public static IDictionary<TNewKey, TValue> MapKeys<TKey, TValue, TNewKey>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, TNewKey> fn) where TNewKey : notnull
            => dict.Select(x =>
            {
                var newKey = fn(x.Key, x.Value);
                return (key: newKey, value: x.Value);
            }).ToDictionary(x => x.key, x => x.value);

        /// <summary>
        /// Fusionne profondément deux dictionnaires.
        /// Les entrées de <paramref name="source"/> sont fusionnées récursivement dans <paramref name="target"/>.
        /// En cas de conflit sur une clé, la valeur de <paramref name="source"/> est prioritaire,
        /// sauf si les deux valeurs sont elles-mêmes des dictionnaires, auquel cas la fusion est récursive.
        /// </summary>
        /// <typeparam name="TKey">Le type des clés.</typeparam>
        /// <typeparam name="TValue">Le type des valeurs.</typeparam>
        /// <param name="target">Le dictionnaire de base. N'est pas modifié.</param>
        /// <param name="source">Le dictionnaire à fusionner dans <paramref name="target"/>.</param>
        /// <returns>Un nouveau dictionnaire résultant de la fusion profonde.</returns>
        public static IDictionary<TKey, TValue> DeepMerge<TKey, TValue>(
            this IDictionary<TKey, TValue> target,
            IDictionary<TKey, TValue> source) where TKey : notnull
        {
            Dictionary<TKey, TValue> results = new(target);

            foreach (var item in source)
            {
                if (results.TryGetValue(item.Key, out TValue? value)
                    && value is IDictionary targetNested
                    && item.Value is IDictionary sourceNested)
                {
                    results[item.Key] = (TValue)_DeepMergeNonGeneric(targetNested, sourceNested);
                }
                else
                {
                    results[item.Key] = item.Value;
                }
            }

            return results;
        }

        private static IDictionary _DeepMergeNonGeneric(IDictionary target, IDictionary source)
        {
            IDictionary result;
            try { result = (IDictionary)Activator.CreateInstance(target.GetType())!; }
            catch { result = new Dictionary<object, object?>(); }

            foreach (DictionaryEntry entry in target)
                result[entry.Key] = entry.Value;

            foreach (DictionaryEntry entry in source)
            {
                if (result.Contains(entry.Key)
                    && result[entry.Key] is IDictionary nestedTarget
                    && entry.Value is IDictionary nestedSource)
                {
                    result[entry.Key] = _DeepMergeNonGeneric(nestedTarget, nestedSource);
                }
                else
                {
                    result[entry.Key] = entry.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Méthode récursive interne pour <see cref="FlattenObject"/>.
        /// Parcourt <paramref name="current"/> et écrit les feuilles dans <paramref name="result"/>
        /// avec leur chemin complet comme clé.
        /// </summary>
        /// <param name="result">Le dictionnaire de résultat à alimenter (passé par référence).</param>
        /// <param name="separator">Le séparateur de segments de clé.</param>
        /// <param name="current">Le dictionnaire courant à parcourir.</param>
        /// <param name="prefix">Le préfixe de clé accumulé lors de la récursion.</param>
        private static void _Flatten(ref Dictionary<string, object?> result, string separator, IDictionary<string, object?> current, string prefix)
        {
            foreach (var kvp in current)
            {
                var newKey = prefix.Length > 0 ? $"{prefix}{separator}{kvp.Key}" : kvp.Key;

                if (kvp.Value is IDictionary<string, object?> nested)
                    _Flatten(ref result, separator, nested, newKey);
                else
                    result[newKey] = kvp.Value;
            }
        }

        /// <summary>
        /// Aplatit un dictionnaire imbriqué en un dictionnaire à un seul niveau,
        /// en concaténant les clés avec le séparateur spécifié.
        /// </summary>
        /// <example>
        /// <code>
        /// // { "a": { "b": { "c": 1 } } } → { "a.b.c": 1 }
        /// var flat = nested.FlattenObject();
        /// </code>
        /// </example>
        /// <param name="obj">Le dictionnaire imbriqué à aplatir.</param>
        /// <param name="separator">Le séparateur utilisé pour construire les clés composées. Par défaut : <c>"."</c>.</param>
        /// <returns>Un nouveau dictionnaire plat dont les clés représentent les chemins vers chaque valeur feuille.</returns>
        public static IDictionary<string, object?> FlattenObject(
            this IDictionary<string, object?> obj,
            string separator = ".")
        {
            var result = new Dictionary<string, object?>();

            _Flatten(ref result, separator, obj, "");
            return result;
        }

        /// <summary>
        /// Reconstruit un dictionnaire imbriqué à partir d'un dictionnaire plat
        /// dont les clés encodent la profondeur via un séparateur.
        /// Opération inverse de <see cref="FlattenObject"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// // { "a.b.c": 1 } → { "a": { "b": { "c": 1 } } }
        /// var nested = flat.UnflattenObject();
        /// </code>
        /// </example>
        /// <remarks>
        /// Si une clé intermédiaire existe déjà avec une valeur qui n'est pas un dictionnaire,
        /// elle est écrasée par un nouveau dictionnaire.
        /// </remarks>
        /// <param name="obj">Le dictionnaire plat à reconstruire.</param>
        /// <param name="separator">Le séparateur utilisé pour découper les clés. Par défaut : <c>"."</c>.</param>
        /// <returns>Un nouveau dictionnaire imbriqué reconstruit à partir des chemins de clés.</returns>
        public static IDictionary<string, object?> UnflattenObject(
            this IDictionary<string, object?> obj,
            string separator = ".")
        {
            var result = new Dictionary<string, object?>();

            foreach (var kvp in obj)
            {
#if NETSTANDARD2_0
                var parts = kvp.Key.Split(separator.ToCharArray());
#else
                var parts = kvp.Key.Split(separator);
#endif
                IDictionary<string, object?> current = result;

                for (int i = 0; i < parts.Length - 1; ++i)
                {
                    var part = parts[i];
                    if (!current.TryGetValue(part, out var existing) || existing is not IDictionary<string, object?> nested)
                    {
                        nested = new Dictionary<string, object?>();
                        current[part] = nested;
                    }
                    current = nested;
                }
#if NETSTANDARD2_0
                current[parts[parts.Length - 1]] = kvp.Value;
#else
                current[parts[^1]] = kvp.Value;
#endif
            }

            return result;
        }

        /// <summary>
        /// Filtre les entrées du dictionnaire en ne conservant que celles dont la clé satisfait le prédicat.
        /// </summary>
        /// <typeparam name="TKey">Le type des clés.</typeparam>
        /// <typeparam name="TValue">Le type des valeurs.</typeparam>
        /// <param name="dict">Le dictionnaire source.</param>
        /// <param name="fn">Prédicat appliqué à chaque clé. Retourne <c>true</c> pour conserver l'entrée.</param>
        /// <returns>Un nouveau dictionnaire contenant uniquement les entrées dont la clé satisfait <paramref name="fn"/>.</returns>
        public static IDictionary<TKey, TValue> FilterKeys<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, bool> fn) where TKey : notnull
            => dict.Where(x => fn(x.Key)).ToDictionary(x => x.Key, x => x.Value);

        /// <summary>
        /// Filtre les entrées du dictionnaire en ne conservant que celles dont la valeur satisfait le prédicat.
        /// </summary>
        /// <typeparam name="TKey">Le type des clés.</typeparam>
        /// <typeparam name="TValue">Le type des valeurs.</typeparam>
        /// <param name="dict">Le dictionnaire source.</param>
        /// <param name="fn">Prédicat appliqué à chaque valeur. Retourne <c>true</c> pour conserver l'entrée.</param>
        /// <returns>Un nouveau dictionnaire contenant uniquement les entrées dont la valeur satisfait <paramref name="fn"/>.</returns>
        public static IDictionary<TKey, TValue> FilterValues<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TValue, bool> fn) where TKey : notnull
            => dict.Where(x => fn(x.Value)).ToDictionary(x => x.Key, x => x.Value);

        /// <summary>
        /// Retourne un nouveau dictionnaire dont les clés et les valeurs sont inversées.
        /// </summary>
        /// <remarks>
        /// Si le dictionnaire source contient des valeurs dupliquées, une exception sera levée
        /// lors de la construction du dictionnaire résultant (clés dupliquées).
        /// </remarks>
        /// <typeparam name="TKey">Le type des clés sources, qui devient le type des valeurs résultantes.</typeparam>
        /// <typeparam name="TValue">Le type des valeurs sources, qui devient le type des clés résultantes.</typeparam>
        /// <param name="dict">Le dictionnaire à inverser.</param>
        /// <returns>Un nouveau dictionnaire avec les clés et valeurs permutées.</returns>
        public static IDictionary<TValue, TKey> Invert<TKey, TValue>(this IDictionary<TKey, TValue> dict) where TKey : notnull where TValue : notnull
            => dict.ToDictionary(x => x.Value, x => x.Key);
    }
}