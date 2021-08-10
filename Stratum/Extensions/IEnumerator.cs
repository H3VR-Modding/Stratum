using System;
using System.Collections;

namespace Stratum.Extensions
{
	/// <summary>
	///     Extension methods pertaining to <see cref="IEnumerator" />
	/// </summary>
	public static class ExtIEnumerator
	{
		private static readonly Action NopAction = () => { };

		private static Func<IEnumerator> NopEnumerator(Action action)
		{
			IEnumerator SuccessEnumerator()
			{
				action();

				yield break;
			}

			return SuccessEnumerator;
		}

		private static Func<TException, bool> NopCatch<TException>(Action<TException> @catch)
		{
			return e =>
			{
				@catch(e);
				return false;
			};
		}

		/// <summary>
		///     Allows an <see cref="IEnumerator" /> to be foreach'd. Not to be called manually.
		/// </summary>
		public static IEnumerator GetEnumerator(this IEnumerator @this)
		{
			return @this;
		}

		/// <summary>
		///     Try-finally block for <see cref="IEnumerator" />
		/// </summary>
		/// <param name="this"></param>
		/// <param name="finally">The method to call once the enumerator has finished or thrown an exception</param>
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

		/// <summary>
		///     Try-catch block for <see cref="IEnumerator" />. This overload allows for re-throwing.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="catch">
		///     The method to call once the exception has been thrown. Return <see langword="true" /> to re-throw the
		///     exception.
		/// </param>
		/// <param name="success">
		///		The method to call once the enumerator has completed and no exception has been thrown.
		/// </param>
		/// <typeparam name="TException">The type of exception to catch</typeparam>
		public static IEnumerator TryCatch<TException>(this IEnumerator @this, Func<TException, bool> @catch, Func<IEnumerator> success) where TException : Exception
		{
			var threw = false;

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

					threw = true;
					return false;
				}
			}

			while (MoveNext())
				yield return @this.Current;

			if (threw)
				yield break;

			foreach (object? item in success())
				yield return item;
		}

		/// <inheritdoc cref="TryCatch{TException}(System.Collections.IEnumerator,System.Func{TException,bool},System.Func{System.Collections.IEnumerator})"/>
		public static IEnumerator TryCatch<TException>(this IEnumerator @this, Func<TException, bool> @catch, Action success) where TException : Exception
		{
			return @this.TryCatch(@catch, NopEnumerator(success));
		}

		/// <inheritdoc cref="TryCatch{TException}(System.Collections.IEnumerator,System.Func{TException,bool},System.Func{System.Collections.IEnumerator})"/>
		public static IEnumerator TryCatch<TException>(this IEnumerator @this, Func<TException, bool> @catch) where TException : Exception
		{
			return @this.TryCatch(@catch, NopAction);
		}

		/// <summary>
		///     Try-catch block for <see cref="IEnumerator" />. This overload swallows the exception
		/// </summary>
		/// <param name="this"></param>
		/// <param name="catch">The method to call once the exception has been thrown</param>
		/// <param name="success">
		///		The method to call once the enumerator has completed and no exception has been thrown.
		/// </param>
		/// <typeparam name="TException">The type of exception to catch</typeparam>
		public static IEnumerator TryCatch<TException>(this IEnumerator @this, Action<TException> @catch, Func<IEnumerator> success) where TException : Exception
		{
			return @this.TryCatch(NopCatch(@catch), success);
		}

		/// <inheritdoc cref="TryCatch{TException}(System.Collections.IEnumerator,System.Action{TException},System.Func{System.Collections.IEnumerator})"/>
		public static IEnumerator TryCatch<TException>(this IEnumerator @this, Action<TException> @catch, Action success) where TException : Exception
		{
			return @this.TryCatch(NopCatch(@catch), NopEnumerator(success));
		}

		/// <inheritdoc cref="TryCatch{TException}(System.Collections.IEnumerator,System.Action{TException},System.Func{System.Collections.IEnumerator})"/>
		public static IEnumerator TryCatch<TException>(this IEnumerator @this, Action<TException> @catch) where TException : Exception
		{
			return @this.TryCatch(NopCatch(@catch), NopAction);
		}

		/// <inheritdoc cref="TryCatch{TException}(System.Collections.IEnumerator,System.Func{TException,bool},System.Func{System.Collections.IEnumerator})"/>
		public static IEnumerator TryCatch(this IEnumerator @this, Func<Exception, bool> @catch, Func<IEnumerator> success)
		{
			return @this.TryCatch<Exception>(@catch, success);
		}

		/// <inheritdoc cref="TryCatch{TException}(System.Collections.IEnumerator,System.Func{TException,bool},System.Func{System.Collections.IEnumerator})"/>
		public static IEnumerator TryCatch(this IEnumerator @this, Func<Exception, bool> @catch, Action success)
		{
			return @this.TryCatch<Exception>(@catch, success);
		}

		/// <inheritdoc cref="TryCatch{TException}(System.Collections.IEnumerator,System.Func{TException,bool},System.Func{System.Collections.IEnumerator})"/>
		public static IEnumerator TryCatch(this IEnumerator @this, Func<Exception, bool> @catch)
		{
			return @this.TryCatch<Exception>(@catch);
		}

		/// <inheritdoc cref="TryCatch{TException}(System.Collections.IEnumerator,System.Action{TException},System.Func{System.Collections.IEnumerator})"/>
		public static IEnumerator TryCatch(this IEnumerator @this, Action<Exception> @catch, Func<IEnumerator> success)
		{
			return @this.TryCatch<Exception>(@catch, success);
		}

		/// <inheritdoc cref="TryCatch{TException}(System.Collections.IEnumerator,System.Action{TException},System.Func{System.Collections.IEnumerator})"/>
		public static IEnumerator TryCatch(this IEnumerator @this, Action<Exception> @catch, Action success)
		{
			return @this.TryCatch<Exception>(@catch, success);
		}

		/// <inheritdoc cref="TryCatch{TException}(System.Collections.IEnumerator,System.Action{TException},System.Func{System.Collections.IEnumerator})"/>
		public static IEnumerator TryCatch(this IEnumerator @this, Action<Exception> @catch)
		{
			return @this.TryCatch<Exception>(@catch);
		}

		/// <summary>
		///     Merges two <see cref="IEnumerator" />s into one
		/// </summary>
		/// <param name="this"></param>
		/// <param name="continuation">The enumerator to enumerate after this enumerator finishes</param>
		public static IEnumerator ContinueWith(this IEnumerator @this, IEnumerator continuation)
		{
			foreach (object? item in @this)
				yield return item;

			foreach (object? item in continuation)
				yield return item;
		}

		/// <summary>
		///     Runs an action after an <see cref="IEnumerator" /> finishes
		/// </summary>
		/// <param name="this"></param>
		/// <param name="continuation">The action to run after this enumerator finishes</param>
		public static IEnumerator ContinueWith(this IEnumerator @this, Action continuation)
		{
			foreach (object? item in @this)
				yield return item;

			continuation();
		}
	}
}
