using System.Collections.Generic;

namespace CachedResourceManager
{
    public static class DictionaryExtensions
    {
        public static TValue TryFindValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : class
        {
            return TryFindValue(dictionary, key, null);
        }

        public static TValue TryFindValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue fallbackValue)
        {
            TValue result;
            return dictionary.TryGetValue(key, out result) ? result : fallbackValue;
        }
    }
}
