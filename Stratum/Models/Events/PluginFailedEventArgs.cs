using System;
using System.Collections.Generic;

namespace Stratum
{
	public sealed class PluginFailedEventArgs : EventArgs, ILoadedPlugin
	{
		internal PluginFailedEventArgs(TimeSpan duration, Stages stage, IReadOnlyStratumPlugin plugin,
			IReadOnlyList<IReadOnlyStratumPlugin> dependents, Exception exception)
		{
			Duration = duration;
			Stage = stage;
			Plugin = plugin;
			Dependents = dependents;
			Exception = exception;
		}

		public TimeSpan Duration { get; }
		public Stages Stage { get; }
		public IReadOnlyStratumPlugin Plugin { get; }
		public bool Success => false;
		public IReadOnlyList<IReadOnlyStratumPlugin> Dependents { get; }
		public Exception Exception { get; }
	}
}
