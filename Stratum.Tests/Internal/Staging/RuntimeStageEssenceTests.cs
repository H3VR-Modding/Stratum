using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using Stratum.Internal.Staging;
using Xunit;

namespace Stratum.Tests.Internal.Staging
{
	public class RuntimeStageEssenceTests
	{
		[Fact]
		private void Variant_IsRuntime()
		{
			RuntimeStageEssence essence = new();

			Stages ret = essence.Variant;

			Assert.Equal(Stages.Runtime, ret);
		}

		[Fact]
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private void Run_Success_CallbackMade()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);
			Expression<Func<IStratumPlugin, IEnumerator>> onRuntime = x => x.OnRuntime(It.IsAny<IStageContext<IEnumerator>>());

			IStage<IEnumerator> stage = Mock.Of<IStage<IEnumerator>>(MockBehavior.Strict);
			Mock<IStratumPlugin> plugin = new();
			StageContext<IEnumerator> ctx = new(stage, plugin.Object);
			RuntimeStageEssence essence = new();
			Mock<Action> callback = new();

			plugin.Setup(onRuntime)
				.Returns(set.GetEnumerator);

			IEnumerator ret = essence.Run(ctx, callback.Object);

			ret.AssertEqual(set);
			plugin.Verify(onRuntime, Times.Once);
			plugin.VerifyNoOtherCalls();
			callback.Verify(x => x(), Times.Once);
			callback.VerifyNoOtherCalls();
		}
	}
}
