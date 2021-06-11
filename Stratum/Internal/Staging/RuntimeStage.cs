using System;
using System.Collections;
using BepInEx.Logging;
using Stratum.Extensions;

namespace Stratum.Internal.Staging
{
	internal sealed class RuntimeStage : Stage<IEnumerator>
	{
		public RuntimeStage(int count, ManualLogSource logger) : base(count, logger) { }

		public override Stages StageType => Stages.Runtime;

		protected override IEnumerator BeginRun(StageContext<IEnumerator> ctx)
		{
			var plugin = ctx.Plugin;

			IEnumerator enumerator;
			try
			{
				enumerator = plugin.OnRuntime(ctx);
			}
			catch (Exception e)
			{
				throw new Exception("The plugin's runtime callback threw an exception (pre-yield).", e);
			}

			foreach (var item in enumerator.TryCatch(e => throw new Exception("The plugin's runtime callback threw an exception (mid-yield)", e)))
				yield return item;

			// Do try-finally this. Context should only be added through a successful load.
			EndRun(ctx);
		}
	}
}
