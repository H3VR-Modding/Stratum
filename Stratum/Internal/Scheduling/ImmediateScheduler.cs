using System;
using System.Linq;
using BepInEx.Logging;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Staging;

namespace Stratum.Internal.Scheduling
{
	internal sealed class ImmediateScheduler : Scheduler<Empty>
	{
		public ImmediateScheduler(ManualLogSource logger, DependencyEnumerable<IStratumPlugin> mods) : base(logger, mods)
		{
		}

		public override Empty Run(Stage<Empty> stage)
		{
			foreach (var mod in Mods.SelectMany(v => v))
				try
				{
					stage.Run(mod.Metadata);
				}
				catch (Exception e)
				{
					ContextException(mod, stage, e);
				}

			return new();
		}
	}
}
