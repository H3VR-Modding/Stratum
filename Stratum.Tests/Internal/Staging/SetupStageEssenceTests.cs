using System;
using System.Linq.Expressions;
using Moq;
using Stratum.Internal.Staging;
using Xunit;

namespace Stratum.Tests.Internal.Staging
{
	public class SetupStageEssenceTests
	{
		[Fact]
		private void Variant_IsSetup()
		{
			SetupStageEssence essence = new();

			Stages ret = essence.Variant;

			Assert.Equal(Stages.Setup, ret);
		}

		[Fact]
		private void Run_Success_CallbackMade()
		{
			IStage<Empty> stage = Mock.Of<IStage<Empty>>();
			Mock<IStratumPlugin> plugin = new();
			StageContext<Empty> ctx = new(stage, plugin.Object);
			SetupStageEssence essence = new();
			Mock<Action<StageContext<Empty>>> callback = new();

			essence.Run(ctx, callback.Object);

			plugin.Verify(x => x.OnSetup(It.IsAny<StageContext<Empty>>()), Times.Once);
			plugin.VerifyNoOtherCalls();
			callback.Verify(x => x(ctx), Times.Once);
			callback.VerifyNoOtherCalls();
		}

		[Fact]
		private void Run_Throw_Throw()
		{
			IStage<Empty> stage = Mock.Of<IStage<Empty>>();
			Mock<IStratumPlugin> plugin = new();
			StageContext<Empty> ctx = new(stage, plugin.Object);
			SetupStageEssence essence = new();
			Action<StageContext<Empty>> callback = Mock.Of<Action<StageContext<Empty>>>();

			plugin.Setup(x => x.OnSetup(It.IsAny<StageContext<Empty>>()))
				.Throws<TestException>()
				.Verifiable();

			Assert.Throws<Exception>(() => essence.Run(ctx, callback));

			plugin.Verify();
		}
	}
}
