using System.Collections.Generic;

namespace Stratum
{
	public interface IReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		int Count { get; }

		IEnumerable<TKey> Keys { get; }

		IEnumerable<TValue> Values { get; }

		TValue this[TKey key] { get; }

		bool ContainsKey(TKey key);

		bool TryGetValue(TKey key, out TValue value);
	}
}
