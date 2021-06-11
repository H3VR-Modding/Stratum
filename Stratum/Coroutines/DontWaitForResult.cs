using System;

namespace Stratum.Coroutines
{
	public sealed class DontWaitForResult<T> : IResultEnumerator<T>
	{
		public T Value { get; }

		public object Current => throw new NotSupportedException();

		public DontWaitForResult(T value) => Value = value;

		public bool MoveNext() => false;

		public void Reset() => throw new NotSupportedException();
	}
}
