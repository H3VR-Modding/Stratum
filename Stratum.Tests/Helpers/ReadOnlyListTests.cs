using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using Xunit;

namespace Stratum.Tests.Helpers
{
	public class ReadOnlyListTests
	{
		[Fact]
		public void Count()
		{
			Mock<IList<object>> mut = new(MockBehavior.Strict);
			IReadOnlyList<object> read = new ReadOnlyList<object>(mut.Object);

			Expression<Func<IList<object>, int>> expr = @this => @this.Count;
			const int output = 3;
			mut.SetupGet(expr).Returns(output);

			int ret = read.Count;

			Assert.Equal(output, ret);
			mut.VerifyGet(expr, Times.Once);
		}

		[Fact]
		public void Indexer()
		{
			Mock<IList<object>> mut = new(MockBehavior.Strict);
			IReadOnlyList<object> read = new ReadOnlyList<object>(mut.Object);

			const int input = 2;
			Expression<Func<IList<object>, object>> expr = @this => @this[input];
			object output = new();
			mut.Setup(expr).Returns(output);

			object ret = read[input];
			Assert.Equal(output, ret);

			mut.Verify(expr, Times.Once);
		}

		[Fact]
		public void Identical()
		{
			Mock<IList<object>> mut = new(MockBehavior.Strict);
			IReadOnlyList<object> read = new ReadOnlyList<object>(mut.Object);

			Expression<Func<IList<object>, IEnumerator<object>>> expr = @this => @this.GetEnumerator();
			IEnumerator<object> output = Mock.Of<IEnumerator<object>>();
			mut.Setup(expr).Returns(output);

			IEnumerator<object> ret = read.GetEnumerator();

			Assert.Equal(output, ret);
			mut.Verify(expr, Times.Once);
		}
	}
}
