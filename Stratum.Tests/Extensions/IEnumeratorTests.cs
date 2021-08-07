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
		[Fact]
		public void GetEnumerator_ReturnsSelf()
		{
			IEnumerator enumerator = Enumerable.Range(0, 10).GetEnumerator();

			IEnumerator ret = enumerator.GetEnumerator();

			Assert.Equal(enumerator, ret);
		}

		[Fact]
		public void ContinueWith_Action()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			IEnumerator enumerator = set.GetEnumerator();
			Mock<Action> continuation = new();

			IEnumerator ret = enumerator.ContinueWith(continuation.Object);

			ret.AssertEqual(set);
			continuation.Verify(x => x(), Times.Once);
			continuation.VerifyNoOtherCalls();
		}

		[Fact]
		public void ContinueWith_IEnumerator()
		{
			IEnumerable<int> first = Enumerable.Range(0, 10);
			IEnumerable<int> second = Enumerable.Range(20, 10);
			IEnumerator firstEnumerator = first.GetEnumerator();
			IEnumerator secondEnumerator = second.GetEnumerator();

			IEnumerator ret = firstEnumerator.ContinueWith(secondEnumerator);

			ret.AssertEqual(first.Concat(second));
		}

		[Fact]
		public void TryCatch_Action_Success()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			IEnumerator enumerator = set.GetEnumerator();
			Action<Exception> @catch = Mock.Of<Action<Exception>>(MockBehavior.Strict);

			IEnumerator ret = enumerator.TryCatch(@catch);

			ret.AssertEqual(set);
		}

		[Fact]
		public void TryCatch_Action_Throw()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);

			IEnumerator enumerator = set.EndWithThrow();
			Mock<Action<Exception>> @catch = new();

			IEnumerator ret = enumerator.TryCatch(@catch.Object);

			ret.AssertEqual(set);
			@catch.Verify(x => x(It.IsAny<TestException>()), Times.Once);
			@catch.VerifyNoOtherCalls();
		}

		[Fact]
		public void TryCatch_Func_Success()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			IEnumerator enumerator = set.GetEnumerator();
			Func<Exception, bool> @catch = Mock.Of<Func<Exception, bool>>(MockBehavior.Strict);

			IEnumerator ret = enumerator.TryCatch(@catch);

			ret.AssertEqual(set);
		}

		[Fact]
		public void TryCatch_Func_Throw_Catch()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			Expression<Func<Func<Exception, bool>, bool>> invoke = x => x(It.IsAny<TestException>());

			IEnumerator enumerator = set.EndWithThrow();
			Mock<Func<Exception, bool>> @catch = new();
			@catch.Setup(invoke)
				.Returns(false);

			IEnumerator ret = enumerator.TryCatch(@catch.Object);

			ret.AssertEqual(set);
			@catch.Verify(invoke, Times.Once);
			@catch.VerifyNoOtherCalls();
		}

		[Fact]
		public void TryCatch_Func_Throw_Rethrow()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			Expression<Func<Func<Exception, bool>, bool>> invoke = x => x(It.IsAny<TestException>());

			IEnumerator enumerator = set.EndWithThrow();
			Mock<Func<Exception, bool>> @catch = new();
			@catch.Setup(invoke)
				.Returns(true)
				.Verifiable();

			IEnumerator ret = enumerator.TryCatch(@catch.Object);

			Assert.Throws<TestException>(() => ret.AssertEqual(set));
			@catch.Verify(invoke, Times.Once);
			@catch.VerifyNoOtherCalls();
		}

		[Fact]
		public void TryFinally_Success()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			IEnumerator enumerator = set.GetEnumerator();
			Mock<Action> @finally = new();

			IEnumerator ret = enumerator.TryFinally(@finally.Object);

			ret.AssertEqual(set);
			@finally.Verify(x => x(), Times.Once);
			@finally.VerifyNoOtherCalls();
		}

		[Fact]
		public void TryFinally_Throw()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);

			IEnumerator enumerator = set.EndWithThrow();
			Mock<Action> @finally = new();

			IEnumerator ret = enumerator.TryFinally(@finally.Object);

			Assert.Throws<TestException>(() => ret.AssertEqual(set));
			@finally.Verify(x => x(), Times.Once);
			@finally.VerifyNoOtherCalls();
		}
	}
}
