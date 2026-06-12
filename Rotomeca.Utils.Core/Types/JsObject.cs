using System.Collections;
using System.Dynamic;
using Rotomeca.Core.Collections;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace Rotomeca.Utils.Types
{
#if NET8_0_OR_GREATER
/// <summary>
/// Représente un objet dynamique à la manière d'un objet JavaScript :
/// collection de propriétés nommées (<c>string</c>) à valeurs de types quelconques.
/// </summary>
/// <remarks>
/// <para>
/// Supporte la syntaxe pointée via <c>dynamic</c> :
/// <code>
/// dynamic obj = new JsObject { ["a"] = 5 };
/// obj.a = 58;
/// </code>
/// </para>
/// <para>
/// Trois stratégies de cache sont employées :
/// <list type="number">
///   <item><description>
///     <b>Cache de types</b> : le <see cref="Type"/> de chaque valeur est mémorisé à l'écriture,
///     évitant tout appel à <c>GetType()</c> lors des accès typés.
///   </description></item>
///   <item><description>
///     <b>Cache de représentations string</b> : le résultat de <c>ToString()</c> est mémorisé
///     par clé et invalidé uniquement à la prochaine écriture sur cette clé.
///   </description></item>
///   <item><description>
///     <b>Snapshot figé (NET8+)</b> : <see cref="Freeze"/> convertit le store interne en
///     <c>FrozenDictionary</c>, optimisé pour les lectures intensives.
///     Toute écriture ultérieure invalide automatiquement le snapshot.
///   </description></item>
/// </list>
/// </para>
/// </remarks>
#else
    /// <summary>
    /// Représente un objet dynamique à la manière d'un objet JavaScript :
    /// collection de propriétés nommées (<c>string</c>) à valeurs de types quelconques.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supporte la syntaxe pointée via <c>dynamic</c> :
    /// <code>
    /// dynamic obj = new JsObject { ["a"] = 5 };
    /// obj.a = 58;
    /// </code>
    /// </para>
    /// <para>
    /// Trois stratégies de cache sont employées :
    /// <list type="number">
    ///   <item><description>
    ///     <b>Cache de types</b> : le <see cref="Type"/> de chaque valeur est mémorisé à l'écriture,
    ///     évitant tout appel à <c>GetType()</c> lors des accès typés.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Cache de représentations string</b> : le résultat de <c>ToString()</c> est mémorisé
    ///     par clé et invalidé uniquement à la prochaine écriture sur cette clé.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Snapshot figé (NET8+)</b> : <c>Freeze</c> convertit le store interne en
    ///     <c>FrozenDictionary</c>, optimisé pour les lectures intensives.
    ///     Toute écriture ultérieure invalide automatiquement le snapshot.
    ///   </description></item>
    /// </list>
    /// </para>
    /// </remarks>
#endif  
    public class JsObject : DynamicObject, IEnumerable<KeyValuePair<string, object?>>
    {
        // ── Store principal ───────────────────────────────────────────────
        private readonly Dictionary<string, object?> _store;

        // ── Stratégie 1 : cache des types (évite GetType() répété) ────────
        private readonly Dictionary<string, Type?> _typeCache;

        // ── Stratégie 2 : cache des représentations string ────────────────
        private Dictionary<string, string?>? _stringCache;

#if NET8_0_OR_GREATER
        // ── Stratégie 3 : snapshot figé pour lectures intensives ──────────
        private FrozenDictionary<string, object?>? _frozen;
        private bool _isDirty;
#endif

        /// <summary>
        /// Initialise un nouvel objet vide avec une capacité initiale optionnelle.
        /// </summary>
        /// <param name="capacity">
        /// Capacité initiale du store. Défaut : <c>4</c> (taille typique d'un petit objet JS).
        /// </param>
        public JsObject(int capacity = 4)
        {
            _store = new(capacity);
            _typeCache = new(capacity);
        }

        /// <summary>
        /// Obtient ou définit la valeur associée à la clé <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Nom de la propriété.</param>
        /// <returns>
        /// La valeur associée, ou <c>null</c> si la clé est absente
        /// (comportement identique à JavaScript).
        /// </returns>
        public object? this[string key]
        {
            get => _Get(key);
            set => _Set(key, value);
        }

        /// <summary>Nombre de propriétés dans l'objet.</summary>
        public int Count => _store.Count;

        // ── Accès interne ─────────────────────────────────────────────────

        private object? _Get(string key)
        {
#if NET8_0_OR_GREATER
            if (_frozen is not null && !_isDirty)
            {
                _frozen.TryGetValue(key, out var frozen);
                return frozen;
            }
#endif
            _store.TryGetValue(key, out var value);
            return value;
        }

        private void _Set(string key, object? value)
        {
            _store[key] = value;
            _typeCache[key] = value?.GetType();

            _stringCache?.Remove(key);

#if NET8_0_OR_GREATER
            _frozen = null;
            _isDirty = true;
#endif
        }

        // ── Lecture ───────────────────────────────────────────────────────

        /// <summary>
        /// Retourne la valeur associée à <paramref name="key"/> castée en <typeparamref name="T"/>,
        /// ou <c>default</c> si la clé est absente ou le type incompatible.
        /// </summary>
        /// <typeparam name="T">Type attendu de la valeur.</typeparam>
        /// <param name="key">Nom de la propriété.</param>
        public T? Get<T>(string key)
        {
            var value = _Get(key);
            return value is T typed ? typed : default;
        }

        /// <summary>
        /// Retourne le <see cref="Type"/> de la valeur associée à <paramref name="key"/>
        /// depuis le cache de types, sans appel à <c>GetType()</c>.
        /// </summary>
        /// <param name="key">Nom de la propriété.</param>
        /// <returns>Le type mis en cache, ou <c>null</c> si la clé est absente ou la valeur nulle.</returns>
        public Type? TypeOf(string key)
        {
            _typeCache.TryGetValue(key, out var type);
            return type;
        }

        /// <summary>
        /// Retourne la représentation string de la valeur associée à <paramref name="key"/>,
        /// depuis le cache string si disponible.
        /// </summary>
        /// <param name="key">Nom de la propriété.</param>
        /// <returns>La chaîne mise en cache, ou <c>null</c> si la valeur est nulle ou la clé absente.</returns>
        public string? Stringify(string key)
        {
            _stringCache ??= [];

            if (_stringCache.TryGetValue(key, out var cached))
                return cached;

            var str = _Get(key)?.ToString();
            _stringCache[key] = str;
            return str;
        }

        /// <summary>Indique si la propriété <paramref name="key"/> existe dans l'objet.</summary>
        public bool Has(string key) => _store.ContainsKey(key);

        // ── Écriture ──────────────────────────────────────────────────────

        /// <summary>
        /// Supprime la propriété <paramref name="key"/> et invalide ses entrées de cache.
        /// </summary>
        /// <param name="key">Nom de la propriété à supprimer.</param>
        /// <returns><c>true</c> si la propriété existait ; sinon <c>false</c>.</returns>
        public bool Delete(string key)
        {
            if (!_store.Remove(key)) return false;

            _typeCache.Remove(key);
            _stringCache?.Remove(key);

#if NET8_0_OR_GREATER
            _frozen = null;
            _isDirty = true;
#endif
            return true;
        }

        /// <summary>
        /// Ajoute ou met à jour la propriété <paramref name="key"/>.
        /// Alias de l'indexeur, requis pour le support de la syntaxe d'initialisation de collection.
        /// </summary>
        public void Add(string key, object? value) => _Set(key, value);

        // ── Itération ─────────────────────────────────────────────────────

        /// <summary>Retourne les noms de toutes les propriétés.</summary>
        public RArray<string> Keys() => new(_store.Keys);

        /// <summary>Retourne les valeurs de toutes les propriétés.</summary>
        public RArray<object?> Values() => new(_store.Values);

        /// <summary>Retourne les paires <c>(clé, valeur)</c> de toutes les propriétés.</summary>
        public RArray<(string Key, object? Value)> Entries()
            => new(_store.Select(kv => (kv.Key, kv.Value)));

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
            => _store.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // ── Dynamic dispatch ──────────────────────────────────────────────

        /// <inheritdoc/>
        /// <remarks>
        /// Permet <c>dynamic obj = new JsObject(); var x = obj.a;</c>.
        /// Retourne toujours <c>true</c> — comme JS, une propriété absente vaut <c>null</c>.
        /// </remarks>
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = _Get(binder.Name);
            return true;
        }

        /// <inheritdoc/>
        /// <remarks>Permet <c>obj.a = 58;</c>.</remarks>
        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            _Set(binder.Name, value);
            return true;
        }

        /// <inheritdoc/>
        /// <remarks>Permet la suppression d'une propriété via <c>dynamic</c>.</remarks>
        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            Delete(binder.Name);
            return true;
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetDynamicMemberNames()
            => _store.Keys;

#if NET8_0_OR_GREATER
        // ── Stratégie 3 : gel du snapshot ─────────────────────────────────

        /// <summary>
        /// Fige l'état courant de l'objet dans un <c>FrozenDictionary</c>
        /// pour optimiser les lectures intensives en lecture seule.
        /// </summary>
        /// <remarks>
        /// Le snapshot est automatiquement invalidé dès la prochaine écriture
        /// (<see cref="this[string]"/> set, <see cref="Delete"/>).
        /// </remarks>
        public void Freeze()
        {
            _frozen = _store.ToFrozenDictionary();
            _isDirty = false;
        }
#endif

        // ── Fabrique ──────────────────────────────────────────────────────

        /// <summary>
        /// Crée un <see cref="JsObject"/> à partir d'une liste de paires <c>(clé, valeur)</c>.
        /// </summary>
        /// <param name="entries">Propriétés initiales.</param>
        /// <returns>Nouvelle instance initialisée.</returns>
        /// <example>
        /// <code>
        /// var obj = JsObject.From(("a", 5), ("b", "string"));
        /// </code>
        /// </example>
        public static JsObject From(params (string Key, object? Value)[] entries)
        {
            var obj = new JsObject(entries.Length);
            foreach (var (key, value) in entries)
                obj[key] = value;
            return obj;
        }
    }
}