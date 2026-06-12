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
    }
}