using System;
using System.Collections;

namespace Stratum.Extensions
{
	public static class ExtIEnumerator
	{
		// This looks goofy but it allows enumerators to be foreach'd, which is actually very helpful
		public static IEnumerator GetEnumerator(this IEnumerator @this)
		{
			return @this;
		}

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

			if (exception != null)
				throw exception;
		}

		public static IEnumerator TryCatch<TException>(this IEnumerator @this, Func<TException, bool> @catch) where TException : Exception
		{
			bool MoveNext()
			{
				try
				{
					return @this.MoveNext();
				}
				catch (TException e)
				{
					if (@catch(e))
						throw;

					return false;
				}
			}

			while (MoveNext())
				yield return @this.Current;
		}

		public static IEnumerator TryCatch<TException>(this IEnumerator @this, Action<TException> @catch) where TException : Exception
		{
			return @this.TryCatch<TException>(e =>
			{
				@catch(e);
				return false;
			});
		}

		public static IEnumerator TryCatch(this IEnumerator @this, Func<Exception, bool> @catch)
		{
			return @this.TryCatch<Exception>(@catch);
		}

		public static IEnumerator TryCatch(this IEnumerator @this, Action<Exception> @catch)
		{
			return @this.TryCatch<Exception>(@catch);
		}

		public static IEnumerator ContinueWith(this IEnumerator @this, IEnumerator continuation)
		{
			foreach (object? item in @this)
				yield return item;

			foreach (object? item in continuation)
				yield return item;
		}

		public static IEnumerator ContinueWith(this IEnumerator @this, Action continuation)
		{
			foreach (object? item in @this)
				yield return item;

			continuation();
		}
	}
}
