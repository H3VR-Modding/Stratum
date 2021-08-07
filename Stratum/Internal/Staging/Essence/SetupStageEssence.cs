using System;

namespace Stratum.Internal.Staging
{
	internal sealed class SetupStageEssence : IStageEssence<Empty>
	{
		public Stages Variant => Stages.Setup;

		public Empty Run(StageContext<Empty> ctx, Action<StageContext<Empty>> callback)
		{
			IStratumPlugin plugin = ctx.Plugin;

			try
			{
				plugin.OnSetup(ctx);
			}
			catch (Exception e)
			{
				throw new Exception("The plugin's setup callback threw an exception.", e);
			}

			// Do try-finally this. Context should only be added through a successful load.
			callback(ctx);

			return new Empty();
		}
	}
}
