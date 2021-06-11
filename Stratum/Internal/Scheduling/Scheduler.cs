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

		protected DependencyEnumerable<IStratumPlugin> Plugins { get; }

		protected Scheduler(ManualLogSource logger, DependencyEnumerable<IStratumPlugin> plugins)
		{
			Logger = logger;
			Plugins = plugins;
		}

		protected void ContextException(Graph<IStratumPlugin, bool>.Node plugin, Stage<TRet> stage, Exception e)
		{
			var killed = Plugins.Kill(plugin).Select(v => v.Metadata.Info.ToString()).Skip(1).ToArray();
			var cascade = string.Join(", ", killed);

			var message = $"{plugin.Metadata.Info} caused an exception during {stage}.";
			if (killed.Length > 0)
				message += $"The following hard-dependents will not be loaded: {cascade}";

			message += "\n" + e;

			Logger.LogError(message);
		}

		public abstract TRet Run(Stage<TRet> stage);
	}
}
