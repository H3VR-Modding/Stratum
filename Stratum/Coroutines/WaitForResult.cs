using System;
using System.Collections;

namespace Stratum.Coroutines
{
	public sealed class WaitForResult<T> : IResultEnumerator<T>
	{
		private readonly Ret _ret;
		private readonly IEnumerator _body;
		private bool _done;
		private Exception? _exception;

		public object? Current => _body.Current;

		public T Value =>
			_done
				? _exception is null
					? _ret.Value
					: throw new InvalidOperationException("The inner enumerator threw an exception.", _exception)
				: throw new InvalidOperationException("The inner enumerator is not yet complete.");

		public WaitForResult(ResultEnumeratorBody<T> body)
		{
			_ret = new();
			_body = body(_ret);
		}

		public bool MoveNext()
		{
			if (_done)
				return false;

			bool next;
			try
			{
				next = _body.MoveNext();
			}
			catch (Exception e)
			{
				_done = true;
				_exception = e;
				throw;
			}

			if (next)
				return true;

			_done = true;

			return false;
		}

		public void Reset() => throw new NotSupportedException();

		private class Ret : IRet<T>
		{
			private T? _value;
			private bool _set;

			public T Value => _set ? _value! : throw new InvalidOperationException("The inner enumerator never set a value.");

			T IRet<T>.Value
			{
				set
				{
					_value = value;
					_set = true;
				}
			}
		}
	}
}
