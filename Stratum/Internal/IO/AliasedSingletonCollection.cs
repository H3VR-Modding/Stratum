using System;
using System.Collections;

namespace Stratum.Internal.IO
{
	internal abstract class AliasedSingletonCollection : ICollection, IFreezable, IDisposable
	{
		private SingletonCollection? _read;
		private SingletonCollection? _write;

		protected SingletonCollection Read => _read ?? throw new ObjectDisposedException(GetType().FullName);
		protected SingletonCollection Write => _write ?? throw new ObjectFrozenException(GetType().FullName);

		public int Count => Read.Count;

		public object SyncRoot => this;

		public bool IsSynchronized => false;

		public AliasedSingletonCollection() => _read = _write = new();

		public void CopyTo(Array array, int index) => Read.CopyTo(array, index);

		public void Freeze()
		{
			_write?.Freeze();

			_write = null;
		}

		public void Dispose()
		{
			_read?.Dispose();

			_read = _write = null;
		}

		public IEnumerator GetEnumerator() => Read.GetEnumerator();
	}
}
