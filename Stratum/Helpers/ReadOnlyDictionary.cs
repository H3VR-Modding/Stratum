using System.Collections;
using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///		An object that implements <see cref="IReadOnlyDictionary{TKey,TValue}"/>
	/// </summary>
	/// <typeparam name="TKey">The type of the keys</typeparam>
	/// <typeparam name="TValue">The type of the values</typeparam>
	public sealed class ReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> _dict;

		/// <summary>
		///		Constructs an instance of <see cref="ReadOnlyDictionary{TKey,TValue}"/>
		/// </summary>
		/// <param name="dict">The dictionary to read from</param>
		public ReadOnlyDictionary(IDictionary<TKey, TValue> dict)
		{
			_dict = dict;
		}

		/// <inheritdoc cref="IReadOnlyCollection{T}.Count"/>
		public int Count => _dict.Count;

		/// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.Keys"/>
		public IEnumerable<TKey> Keys => _dict.Keys;

		/// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.Values"/>
		public IEnumerable<TValue> Values => _dict.Values;

		/// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.this"/>
		public TValue this[TKey key] => _dict[key];

		/// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.ContainsKey"/>
		public bool ContainsKey(TKey key)
		{
			return _dict.ContainsKey(key);
		}

		/// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.TryGetValue"/>
		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dict.TryGetValue(key, out value);
		}

		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
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
