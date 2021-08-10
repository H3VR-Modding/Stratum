using System;
using System.Collections.Generic;

namespace Stratum.Extensions
{
	public static class ExtIEnumerable
	{
		public delegate void ForeachAction<in T>(T item);
		public delegate void ForeachAction<in T, in TState>(TState state, T item);

		public static void Foreach<T>(this IEnumerable<T> @this, ForeachAction<T> action)
		{
			foreach (T item in @this)
				action(item);
		}

		public static void Foreach<T, TState>(this IEnumerable<T> @this, TState state, ForeachAction<T, TState> action)
		{
			foreach (T item in @this)
				action(state, item);
		}
	}
}
