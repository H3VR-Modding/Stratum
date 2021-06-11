using System;
using System.Collections;
using System.Collections.Generic;

namespace Stratum.Internal
{
	internal class DisposableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IFreezable, IDisposable
	{
		private Dictionary<TKey, TValue>? _read;
		private Dictionary<TKey, TValue>? _write;

		private IDictionary<TKey, TValue> Read => _read ?? throw new ObjectDisposedException(GetType().FullName);
		private IDictionary<TKey, TValue> Write => _write ?? throw new ObjectFrozenException(GetType().FullName);

		public int Count => Read.Count;

		public bool IsReadOnly => _write is null && (_read is not null ? true : throw new ObjectDisposedException(GetType().FullName));

		public ICollection<TKey> Keys => Read.Keys;
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

		public ICollection<TValue> Values => Read.Values;
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

		public DisposableDictionary() => _read = _write = new();

		public void Add(TKey key, TValue value) => Write.Add(key, value);
		public void Add(KeyValuePair<TKey, TValue> item) => Write.Add(item);

		public bool Remove(TKey key) => Write.Remove(key);
		public bool Remove(KeyValuePair<TKey, TValue> item) => Write.Remove(item);

		public void Clear() => Write.Clear();

		public bool ContainsKey(TKey key) => Read.ContainsKey(key);
		public bool Contains(KeyValuePair<TKey, TValue> item) => Read.Contains(item);

		public bool TryGetValue(TKey key, out TValue value) => Read.TryGetValue(key, out value);

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Read.CopyTo(array, arrayIndex);

		public TValue this[TKey key]
		{
			get => Read[key];
			set => Write[key] = value;
		}

		public void Freeze() => _write = null;

		public void Dispose() => _read = _write = null;

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Read.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
