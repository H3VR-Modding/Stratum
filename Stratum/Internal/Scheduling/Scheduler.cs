using System;
using System.Linq;
using BepInEx.Logging;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Staging;

namespace Stratum.Internal.Scheduling
{
	internal abstract class Scheduler<TRet>
	{
		protected ManualLogSource Logger { get; }

		protected DependencyEnumerable<IStratumPlugin> Mods { get; }

		protected Scheduler(ManualLogSource logger, DependencyEnumerable<IStratumPlugin> mods)
		{
			Logger = logger;
			Mods = mods;
		}

		protected void ContextException(Graph<IStratumPlugin, bool>.Node mod, Stage<TRet> stage, Exception e)
		{
			var killed = Mods.Kill(mod).Select(v => v.Metadata.Info.ToString()).Skip(1).ToArray();
			var cascade = string.Join(", ", killed);
			
			var message = $"{mod.Metadata.Info} caused an exception during {stage}.";
			if (killed.Length > 0)
				message += $"The following hard-dependents will not be loaded: {cascade}";

			message += "\n" + e;
			
			Logger.LogError(message);
		}

		public abstract TRet Run(Stage<TRet> stage);
	}
}
