using System;
using System.Collections;
using Stratum.Extensions;

namespace Stratum.Coroutines
{
	public interface IResultEnumerator<out T> : IEnumerator
	{
		T Value { get; }
	}

	public static class ExtIResultEnumerator
	{
		public static IResultEnumerator<T> TryCatchResult<T>(this IResultEnumerator<T> @this, Func<Exception, T> @catch)
		{
			IEnumerator Result(IRet<T> ret)
			{
				int MoveNext()
				{
					try
					{
						return @this.MoveNext() ? 1 : 0;
					}
					catch (Exception e)
					{
						ret.Value = @catch(e);
						return -1;
					}
				}

				var next = MoveNext();
				while (next == 1)
				{
					yield return @this.Current;

					next = MoveNext();
				}

				// It threw. Return value already known.
				if (next == -1)
					yield break;

				ret.Value = @this.Value;
			}

			return new WaitForResult<T>(Result);
		}

		public static IResultEnumerator<T> TryFinallyResult<T>(this IResultEnumerator<T> @this, Action @finally)
		{
			IEnumerator Result(IRet<T> ret)
			{
				IEnumerator Continuation()
				{
					ret.Value = @this.Value;
					yield break;
				}

				return @this
					.TryFinally(@finally)
					.ContinueWith(Continuation());
			}

			return new WaitForResult<T>(Result);
		}

		public static IResultEnumerator<TContinuation> ContinueWithResult<T, TContinuation>(this IResultEnumerator<T> @this,
			Func<T, IResultEnumerator<TContinuation>> continuation)
		{
			IEnumerator Result(IRet<TContinuation> ret)
			{
				foreach (var item in @this)
					yield return item;

				var cont = continuation(@this.Value);
				foreach (var item in cont)
					yield return item;

				ret.Value = cont.Value;
			}

			return new WaitForResult<TContinuation>(Result);
		}
	}
}
