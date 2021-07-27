using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///     Represents a dictionary that may be read from, but not written to
	/// </summary>
	public interface IReadOnlyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		/// <inheritdoc cref="ICollection{T}.Count" />
		int Count { get; }

		/// <inheritdoc cref="IDictionary{TKey,TValue}.Keys" />
		IEnumerable<TKey> Keys { get; }

		/// <inheritdoc cref="IDictionary{TKey,TValue}.Values" />
		IEnumerable<TValue> Values { get; }

		/// <inheritdoc cref="IDictionary{TKey,TValue}.this[TKey]" />
		TValue this[TKey key] { get; }

		/// <inheritdoc cref="IDictionary{TKey,TValue}.ContainsKey" />
		bool ContainsKey(TKey key);

		/// <inheritdoc cref="IDictionary{TKey,TValue}.TryGetValue" />
		bool TryGetValue(TKey key, out TValue value);
	}
}
