using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using Stratum.Extensions;
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
			Expression<Func<IStratumPlugin, IEnumerator>> onSetup = x => x.OnRuntime(It.IsAny<IStageContext<IEnumerator>>());

			IStage<IEnumerator> stage = Mock.Of<IStage<IEnumerator>>(MockBehavior.Strict);
			Mock<IStratumPlugin> plugin = new();
			plugin.Setup(onSetup).Returns(set.GetEnumerator);
			StageContext<IEnumerator> ctx = new(stage, plugin.Object);
			RuntimeStageEssence essence = new();
			Mock<Action<StageContext<IEnumerator>>> callback = new();

			IEnumerator ret = essence.Run(ctx, callback.Object);

			ret.AssertEqual(set);
			plugin.Verify(onSetup, Times.Once);
			plugin.VerifyNoOtherCalls();
			callback.Verify(x => x(ctx), Times.Once);
			callback.VerifyNoOtherCalls();
		}

		[Fact]
		private void Run_ThrowPreYield_Throw()
		{
			IStage<IEnumerator> stage = Mock.Of<IStage<IEnumerator>>(MockBehavior.Strict);
			Mock<IStratumPlugin> plugin = new();
			plugin.Setup(x => x.OnRuntime(It.IsAny<StageContext<IEnumerator>>())).Throws<TestException>();
			StageContext<IEnumerator> ctx = new(stage, plugin.Object);
			RuntimeStageEssence essence = new();
			Action<StageContext<IEnumerator>> callback = Mock.Of<Action<StageContext<IEnumerator>>>(MockBehavior.Strict);

			Assert.Throws<Exception>(() => essence.Run(ctx, callback).Enumerate());
		}

		[Fact]
		private void Run_ThrowMidYield_Throw()
		{
			IEnumerable<int> set = Enumerable.Range(0, 10);

			IStage<IEnumerator> stage = Mock.Of<IStage<IEnumerator>>(MockBehavior.Strict);
			Mock<IStratumPlugin> plugin = new();
			StageContext<IEnumerator> ctx = new(stage, plugin.Object);
			RuntimeStageEssence essence = new();
			Action<StageContext<IEnumerator>> callback = Mock.Of<Action<StageContext<IEnumerator>>>(MockBehavior.Strict);

			plugin.Setup(x => x.OnRuntime(ctx)).Returns(() => set.EndWithThrow());

			Assert.Throws<Exception>(() => essence.Run(ctx, callback).AssertEqual(set));
		}
	}
}
