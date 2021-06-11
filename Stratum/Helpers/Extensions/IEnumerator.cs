using System;
using System.Collections;

namespace Stratum.Extensions
{
	public static class ExtIEnumerator
	{
		public static IEnumerator GetEnumerator(this IEnumerator enumerator) => enumerator;

		public static IEnumerator TryFinally(this IEnumerator @this, Action @finally)
		{
			Exception? exception = null;
			bool MoveNext()
			{
				try
				{
					return @this.MoveNext();
				}
				catch (Exception e)
				{
					exception = e;
					return false;
				}
			}

			while (MoveNext())
				yield return @this.Current;

			@finally();

			if (exception is not null)
				throw exception;
		}

		public static IEnumerator TryCatch(this IEnumerator @this, Action<Exception> @catch)
		{
			bool MoveNext()
			{
				try
				{
					return @this.MoveNext();
				}
				catch (Exception e)
				{
					@catch(e);
					return false;
				}
			}

			while (MoveNext())
				yield return @this.Current;
		}

		public static IEnumerator ContinueWith(this IEnumerator @this, IEnumerator continuation)
		{
			foreach (var item in @this)
				yield return item;

			foreach (var item in continuation)
				yield return item;
		}
	}
}
