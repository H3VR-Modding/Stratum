using System;
using System.Linq;
using BepInEx.Logging;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Staging;

namespace Stratum.Internal.Scheduling
{
	internal sealed class ImmediateScheduler : Scheduler<Empty>
	{
		public ImmediateScheduler(ManualLogSource logger, DependencyEnumerable<IStratumPlugin> plugins) : base(logger, plugins) { }

		public override Empty Run(Stage<Empty> stage)
		{
			foreach (Graph<IStratumPlugin, bool>.Node plugin in Plugins.SelectMany(v => v))
				try
				{
					stage.Run(plugin.Metadata);
				}
				catch (Exception e)
				{
					ContextException(plugin, stage, e);
				}

			return new Empty();
		}
	}
}
