using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using Stratum.Extensions;
using Xunit;

namespace Stratum.Tests.Extensions
{
	[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
	public class ExtIEnumeratorTests
	{
		private static IEnumerable<int> ThrowSet(IEnumerable<int> set)
		{
			foreach (int item in set)
				yield return item;

			throw new TestException();
		}

		private void AssertEqualEnumerators(IEnumerator expected, IEnumerator actual)
		{
			bool moveExpected, moveActual;
			while ((moveExpected = expected.MoveNext()) & (moveActual = actual.MoveNext()))
				Assert.Equal(expected.Current, actual.Current);

			Assert.Equal(moveExpected, moveActual);
		}

		[Fact]
		public void ContinueWith_Action()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			IEnumerator enumerator = set.GetEnumerator();
			Mock<Action> continuation = new();

			IEnumerator ret = enumerator.ContinueWith(continuation.Object);

			AssertEqualEnumerators(set.GetEnumerator(), ret);
			continuation.Verify(x => x(), Times.Once);
		}

		[Fact]
		public void ContinueWith_IEnumerator()
		{
			IEnumerable<int> first = Enumerable.Range(0, 10);
			IEnumerable<int> second = Enumerable.Range(20, 10);
			IEnumerator firstEnumerator = first.GetEnumerator();
			IEnumerator secondEnumerator = second.GetEnumerator();

			IEnumerator ret = firstEnumerator.ContinueWith(secondEnumerator);

			AssertEqualEnumerators(first.Concat(second).GetEnumerator(), ret);
		}

		[Fact]
		public void TryCatch_Action_Success()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			IEnumerator enumerator = set.GetEnumerator();
			Mock<Action<Exception>> @catch = new();

			IEnumerator ret = enumerator.TryCatch(@catch.Object);

			AssertEqualEnumerators(set.GetEnumerator(), ret);
			@catch.VerifyNoOtherCalls();
		}

		[Fact]
		public void TryCatch_Action_Throw()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);

			IEnumerator enumerator = ThrowSet(set).GetEnumerator();
			Mock<Action<Exception>> @catch = new(MockBehavior.Strict);
			Expression<Action<Action<Exception>>> invoke = x => x(It.IsAny<TestException>());
			@catch.Setup(invoke).Verifiable();

			IEnumerator ret = enumerator.TryCatch(@catch.Object);

			AssertEqualEnumerators(set.GetEnumerator(), ret);
			@catch.Verify(invoke, Times.Once);
		}

		[Fact]
		public void TryCatch_Func_Success()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			IEnumerator enumerator = set.GetEnumerator();
			Mock<Func<Exception, bool>> @catch = new(MockBehavior.Strict);

			IEnumerator ret = enumerator.TryCatch(@catch.Object);

			AssertEqualEnumerators(set.GetEnumerator(), ret);
			@catch.VerifyNoOtherCalls();
		}

		[Fact]
		public void TryCatch_Func_Throw_Catch()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);

			IEnumerator enumerator = ThrowSet(set).GetEnumerator();
			Mock<Func<Exception, bool>> @catch = new(MockBehavior.Strict);
			Expression<Func<Func<Exception, bool>, bool>> invoke = x => x(It.IsAny<TestException>());
			@catch.Setup(invoke)
				.Returns(false)
				.Verifiable();

			IEnumerator ret = enumerator.TryCatch(@catch.Object);

			AssertEqualEnumerators(set.GetEnumerator(), ret);
			@catch.Verify(invoke, Times.Once);
		}

		[Fact]
		public void TryCatch_Func_Throw_Rethrow()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);

			IEnumerator enumerator = ThrowSet(set).GetEnumerator();
			Mock<Func<Exception, bool>> @catch = new(MockBehavior.Strict);
			Expression<Func<Func<Exception, bool>, bool>> invoke = x => x(It.IsAny<TestException>());
			@catch.Setup(invoke)
				.Returns(true)
				.Verifiable();

			IEnumerator ret = enumerator.TryCatch(@catch.Object);

			Assert.Throws<TestException>(() => AssertEqualEnumerators(set.GetEnumerator(), ret));
			@catch.Verify(invoke, Times.Once);
		}

		[Fact]
		public void TryFinally_Success()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			IEnumerator enumerator = set.GetEnumerator();
			Mock<Action> @finally = new(MockBehavior.Strict);
			Expression<Action<Action>> invoke = x => x();
			@finally.Setup(invoke).Verifiable();

			IEnumerator ret = enumerator.TryFinally(@finally.Object);

			AssertEqualEnumerators(set.GetEnumerator(), ret);
			@finally.Verify(invoke, Times.Once);
		}

		[Fact]
		public void TryFinally_Throw()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);

			IEnumerator enumerator = ThrowSet(set).GetEnumerator();
			Mock<Action> @finally = new(MockBehavior.Strict);
			Expression<Action<Action>> invoke = x => x();
			@finally.Setup(invoke).Verifiable();

			IEnumerator ret = enumerator.TryFinally(@finally.Object);

			Assert.Throws<TestException>(() => AssertEqualEnumerators(set.GetEnumerator(), ret));
			@finally.Verify(invoke, Times.Once);
		}
	}
}
