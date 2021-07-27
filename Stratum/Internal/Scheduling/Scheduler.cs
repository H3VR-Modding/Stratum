using System;
using System.Linq;
using BepInEx.Logging;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Staging;

namespace Stratum.Internal.Scheduling
{
	internal abstract class Scheduler<TRet>
	{
		protected Scheduler(ManualLogSource logger, DependencyEnumerable<Plugin> plugins)
		{
			Logger = logger;
			Plugins = plugins;
		}

		protected ManualLogSource Logger { get; }

		protected DependencyEnumerable<Plugin> Plugins { get; }

		protected void ContextException(Graph<Plugin, bool>.Node plugin, Stage<TRet> stage, Exception e)
		{
			string[] killed = Plugins.Kill(plugin).Select(v => v.Metadata.ToString()).Skip(1).ToArray();
			string cascade = string.Join(", ", killed);

			var message = $"{plugin.Metadata} caused an exception during {stage}.";
			if (killed.Length > 0)
				message += $"The following hard-dependents will not be loaded: {cascade}";

			message += "\n" + e;

			Logger.LogError(message);
		}

		public abstract TRet Run(Stage<TRet> stage);
	}
}
