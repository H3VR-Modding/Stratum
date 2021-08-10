using System;

namespace Stratum.Internal.Staging
{
	internal sealed class SetupStageEssence : IStageEssence<Empty>
	{
		public Stages Variant => Stages.Setup;

		public Empty Run(StageContext<Empty> ctx, Action callback)
		{
			ctx.Plugin.OnSetup(ctx);

			callback();

			return new Empty();
		}
	}
}
