using System;
using System.Collections;
using Stratum.Extensions;

namespace Stratum.Internal.Staging
{
	internal sealed class RuntimeStageEssence : IStageEssence<IEnumerator>
	{
		public Stages Variant => Stages.Runtime;

		public IEnumerator Run(StageContext<IEnumerator> ctx, Action<StageContext<IEnumerator>> callback)
		{
			IStratumPlugin plugin = ctx.Plugin;

			IEnumerator enumerator;
			try
			{
				enumerator = plugin.OnRuntime(ctx);
			}
			catch (Exception e)
			{
				throw new Exception("The plugin's runtime callback threw an exception (pre-yield).", e);
			}

			foreach (object? item in enumerator.TryCatch(e =>
				throw new Exception("The plugin's runtime callback threw an exception (mid-yield)", e)))
				yield return item;

			// Do try-finally this. Context should only be added through a successful load.
			callback(ctx);
		}
	}
}
