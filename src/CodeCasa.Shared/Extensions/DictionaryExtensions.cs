namespace CodeCasa.Shared.Extensions;

public static class DictionaryExtensions
{
    public static Dictionary<TValue, TKey> Inverse<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEqualityComparer<TValue> comparer)
        where TKey : notnull
        where TValue : notnull =>
        dictionary.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, comparer);
}