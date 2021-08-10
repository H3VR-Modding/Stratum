using System;
using System.Collections;
using Stratum.Extensions;

namespace Stratum.Internal.Staging
{
	internal sealed class RuntimeStageEssence : IStageEssence<IEnumerator>
	{
		public Stages Variant => Stages.Runtime;

		public IEnumerator Run(StageContext<IEnumerator> ctx, Action callback)
		{
			foreach (object? item in ctx.Plugin.OnRuntime(ctx))
				yield return item;

			callback();
		}
	}
}
