using System.Collections;
using System.Collections.Generic;

namespace Stratum
{
	public class ReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> _dict;

		public ReadOnlyDictionary(IDictionary<TKey, TValue> dict)
		{
			_dict = dict;
		}

		public int Count => _dict.Count;
		public IEnumerable<TKey> Keys => _dict.Keys;
		public IEnumerable<TValue> Values => _dict.Values;

		public TValue this[TKey key] => _dict[key];

		public bool ContainsKey(TKey key)
		{
			return _dict.ContainsKey(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dict.TryGetValue(key, out value);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _dict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _dict).GetEnumerator();
		}
	}
}
