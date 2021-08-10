using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using Xunit;

namespace Stratum.Tests.Helpers
{
	public class ReadOnlyDictionaryTests
	{
		[Fact]
		public void Count()
		{
			Mock<IDictionary<object, object>> mut = new(MockBehavior.Strict);
			IReadOnlyDictionary<object, object> read = new ReadOnlyDictionary<object, object>(mut.Object);

			Expression<Func<IDictionary<object, object>, int>> expr = @this => @this.Count;
			const int output = 3;
			mut.SetupGet(expr).Returns(output);

			int ret = read.Count;

			Assert.Equal(output, ret);
			mut.VerifyGet(expr, Times.Once);
		}

		[Fact]
		public void Keys()
		{
			Mock<IDictionary<object, object>> mut = new(MockBehavior.Strict);
			IReadOnlyDictionary<object, object> read = new ReadOnlyDictionary<object, object>(mut.Object);

			Expression<Func<IDictionary<object, object>, ICollection<object>>> expr = @this => @this.Keys;
			ICollection<object> output = Mock.Of<ICollection<object>>();
			mut.SetupGet(expr).Returns(output);

			IEnumerable<object> ret = read.Keys;

			Assert.StrictEqual(output, ret);
			mut.VerifyGet(expr, Times.Once);
		}

		[Fact]
		public void Values()
		{
			Mock<IDictionary<object, object>> mut = new(MockBehavior.Strict);
			IReadOnlyDictionary<object, object> read = new ReadOnlyDictionary<object, object>(mut.Object);

			Expression<Func<IDictionary<object, object>, ICollection<object>>> expr = @this => @this.Values;
			ICollection<object> output = Mock.Of<ICollection<object>>();
			mut.SetupGet(expr).Returns(output);

			IEnumerable<object> ret = read.Values;

			Assert.StrictEqual(output, ret);
			mut.VerifyGet(expr, Times.Once);
		}

		[Fact]
		public void Indexer()
		{
			Mock<IDictionary<object, object>> mut = new(MockBehavior.Strict);
			IReadOnlyDictionary<object, object> read = new ReadOnlyDictionary<object, object>(mut.Object);

			object input = new();
			Expression<Func<IDictionary<object, object>, object>> expr = @this => @this[input];
			object output = new();
			mut.Setup(expr).Returns(output);

			object ret = read[input];

			Assert.Equal(output, ret);
			mut.Verify(expr, Times.Once);
		}

		[Fact]
		public void ContainsKey()
		{
			Mock<IDictionary<object, object>> mut = new(MockBehavior.Strict);
			IReadOnlyDictionary<object, object> read = new ReadOnlyDictionary<object, object>(mut.Object);

			object input = new();
			Expression<Func<IDictionary<object, object>, bool>> expr = @this => @this.ContainsKey(input);
			const bool output = false;
			mut.Setup(expr).Returns(output);

			bool ret = read.ContainsKey(input);

			Assert.Equal(output, ret);
			mut.Verify(expr, Times.Once);
		}

		[Fact]
		public void GetEnumerator()
		{
			Mock<IDictionary<object, object>> mut = new(MockBehavior.Strict);
			IReadOnlyDictionary<object, object> read = new ReadOnlyDictionary<object, object>(mut.Object);

			Expression<Func<IDictionary<object, object>, IEnumerator<KeyValuePair<object, object>>>> expr = @this => @this.GetEnumerator();
			IEnumerator<KeyValuePair<object, object>> output = Mock.Of<IEnumerator<KeyValuePair<object, object>>>();
			mut.Setup(expr).Returns(output);

			IEnumerator<KeyValuePair<object,object>> ret = read.GetEnumerator();

			Assert.Equal(output, ret);
			mut.Verify(expr, Times.Once);
		}

		private delegate void TryGetCallback<in TKey, TValue>(TKey key, out TValue value);

		[Fact]
		public void TryGetValue()
		{
			Mock<IDictionary<object, object>> mut = new(MockBehavior.Strict);
			IReadOnlyDictionary<object, object> read = new ReadOnlyDictionary<object, object>(mut.Object);

			object input = new();
			Expression<Func<IDictionary<object, object>, bool>> expr = @this => @this.TryGetValue(input, out It.Ref<object>.IsAny!);
			const bool outputRet = false;
			object outputOut = new();
			mut.Setup(expr)
				.Callback(new TryGetCallback<object, object>(((object _, out object value) => value = outputOut)))
				.Returns(outputRet);

			bool ret = read.TryGetValue(input, out object? retOut);

			Assert.Equal(outputRet, ret);
			Assert.Equal(outputOut, retOut);
			mut.Verify(expr, Times.Once);
		}
	}
}
