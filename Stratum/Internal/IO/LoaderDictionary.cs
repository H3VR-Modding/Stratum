using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Stratum.IO;

namespace Stratum.Internal.IO
{
	// Non-generic as to not bloat statics
	internal static class LoaderDictionary
	{
		private static readonly Regex _loaderRegex = new(@"^[a-zA-Z0-9\._\-]+$");

		public static void ValidateKey(string key)
		{
			if (!_loaderRegex.IsMatch(key))
				throw new FormatException("Loader names can only contain the following characters: a-z A-Z 0-9 . _ -");
		}
	}

	internal class LoaderDictionary<TRet> : IDictionary<string, Loader<TRet>>, IReadOnlyDictionary<string, Loader<TRet>>, IFreezable, IDisposable
	{
		// A shortcut
		private static void ValidateKey(string key) => LoaderDictionary.ValidateKey(key);

		private Dictionary<string, Loader<TRet>>? _read;
		private Dictionary<string, Loader<TRet>>? _write;

		private IDictionary<string, Loader<TRet>> Read => _read ?? throw new ObjectDisposedException(GetType().FullName);
		private IDictionary<string, Loader<TRet>> Write => _write ?? throw new ObjectFrozenException(GetType().FullName);

		public int Count => Read.Count;

		public bool IsReadOnly => _write is null && (_read is not null ? true : throw new ObjectDisposedException(GetType().FullName));

		public ICollection<string> Keys => Read.Keys;
		IEnumerable<string> IReadOnlyDictionary<string, Loader<TRet>>.Keys => Keys;

		public ICollection<Loader<TRet>> Values => Read.Values;
		IEnumerable<Loader<TRet>> IReadOnlyDictionary<string, Loader<TRet>>.Values => Values;

		public LoaderDictionary() => _read = _write = new();

		public void Add(string key, Loader<TRet> value)
		{
			ValidateKey(key);

			Write.Add(key, value);
		}

		public void Add(KeyValuePair<string, Loader<TRet>> item)
		{
			ValidateKey(item.Key);

			Write.Add(item);
		}

		public bool Remove(string key) => Write.Remove(key);
		public bool Remove(KeyValuePair<string, Loader<TRet>> item) => Write.Remove(item);

		public void Clear() => Write.Clear();

		public bool ContainsKey(string key) => Read.ContainsKey(key);
		public bool Contains(KeyValuePair<string, Loader<TRet>> item) => Read.Contains(item);

		public bool TryGetValue(string key, out Loader<TRet> value) => Read.TryGetValue(key, out value);

		public void CopyTo(KeyValuePair<string, Loader<TRet>>[] array, int arrayIndex) => Read.CopyTo(array, arrayIndex);

		public Loader<TRet> this[string key]
		{
			get => Read[key];
			set
			{
				ValidateKey(key);

				Write[key] = value;
			}
		}

		public void Freeze() => _write = null;

		public void Dispose() => _read = _write = null;

		public IEnumerator<KeyValuePair<string, Loader<TRet>>> GetEnumerator() => Read.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
