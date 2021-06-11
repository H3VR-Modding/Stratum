using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Stratum.Internal.IO
{
	internal class SingletonCollection : ICollection, IFreezable, IDisposable
	{
		private Dictionary<Type, object?>? _read;
		private Dictionary<Type, object?>? _write;

		private Dictionary<Type, object?> Read => _read ?? throw new ObjectDisposedException(GetType().FullName);
		private Dictionary<Type, object?> Write => _write ?? throw new ObjectFrozenException(GetType().FullName);

		public int Count => Read.Count;

		public object SyncRoot => this;

		public bool IsSynchronized => false;

		public SingletonCollection() => _read = _write = new();

		public bool Contains<T>() => Read.ContainsKey(typeof(T));

		public void Add<T>([AllowNull] T value) => Write.Add(typeof(T), value);

		[return: MaybeNull]
		public T Get<T>() => TryGet<T>(out var value) ? value : throw new KeyNotFoundException("No singleton exists of type " + typeof(T));

		public bool TryGet<T>([MaybeNull] out T value)
		{
			if (Read.TryGetValue(typeof(T), out var boxed))
			{
				value = (T?) boxed;
				return true;
			}

			value = default;
			return false;
		}

		public void CopyTo(Array array, int index)
		{
			var enumerator = GetEnumerator();
			var length = Read.Count;

			for (var i = 0; i < length; ++i)
			{
				enumerator.MoveNext();
				array.SetValue(enumerator.Current, index + i);
			}
		}

		public IEnumerator GetEnumerator() => Read.Values.GetEnumerator();

		public void Freeze() => _write = null;

		public void Dispose() => _read = _write = null;
	}
}
