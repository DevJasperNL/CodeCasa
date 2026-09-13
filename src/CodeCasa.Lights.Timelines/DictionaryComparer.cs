namespace CodeCasa.Lights.Timelines;

internal sealed class DictionaryComparer<TKey, TValue>(IEqualityComparer<TValue>? valueComparer = null)
    : IEqualityComparer<Dictionary<TKey, TValue>>
    where TKey : notnull
{
    private readonly IEqualityComparer<TValue> _valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

    public bool Equals(Dictionary<TKey, TValue>? x, Dictionary<TKey, TValue>? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        if (x.Count != y.Count) return false;

        foreach (var (key, value) in x)
        {
            if (!y.TryGetValue(key, out var yValue) || !_valueComparer.Equals(value, yValue))
                return false;
        }
        return true;
    }

    public int GetHashCode(Dictionary<TKey, TValue> obj)
    {
        // Combined order-independently: dictionaries that compare equal may enumerate in a different order.
        var hash = 0;
        foreach (var (key, value) in obj)
        {
            hash = unchecked(hash + HashCode.Combine(key, value is null ? 0 : _valueComparer.GetHashCode(value)));
        }
        return hash;
    }
}
