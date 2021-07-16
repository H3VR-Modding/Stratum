using System;
using BepInEx.Logging;

namespace Stratum.Internal.Staging
{
	internal sealed class SetupStage : Stage<Empty>
	{
		public override Stages Variant => Stages.Setup;

		public SetupStage(int count, ManualLogSource logger) : base(count, logger)
		{
		}

		protected override Empty BeginRun(StageContext<Empty> ctx)
		{
			var plugin = ctx.Plugin;

			try
			{
				plugin.OnSetup(ctx);
			}
			catch (Exception e)
			{
				throw new Exception("The plugin's setup callback threw an exception.", e);
			}

			// Do try-finally this. Context should only be added through a successful load.
			EndRun(ctx);

			return new();
		}
	}
}
