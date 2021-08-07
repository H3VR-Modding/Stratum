using System.Collections;
using System.Collections.Generic;
using Stratum.Extensions;
using Xunit;

namespace Stratum.Tests
{
	public static class TestExtensions
	{
		public static T[] Populate<T>(this T[] @this) where T : new()
		{
			for (var i = 0; i < @this.Length; ++i)
				@this[i] = new T();

			return @this;
		}

		public static void Enumerate(this IEnumerator @this)
		{
			foreach (object? item in @this) { }
		}

		public static IEnumerator<T> EndWithThrow<T>(this IEnumerable<T> @this)
		{
			foreach (T item in @this)
				yield return item;

			throw new TestException();
		}

		public static void AssertEqual(this IEnumerator actual, IEnumerable expected)
		{
			IEnumerator enumerator = expected.GetEnumerator();

			bool moveExpected, moveActual;
			while ((moveExpected = enumerator.MoveNext()) & (moveActual = actual.MoveNext()))
				Assert.Equal(enumerator.Current, actual.Current);

			Assert.Equal(moveExpected, moveActual);
		}
	}
}
