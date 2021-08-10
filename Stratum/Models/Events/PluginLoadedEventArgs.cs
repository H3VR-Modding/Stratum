using System;

namespace Stratum
{
	public sealed class PluginLoadedEventArgs : EventArgs, ILoadedPlugin
	{
		internal PluginLoadedEventArgs(TimeSpan duration, Stages stage, IStratumPlugin plugin)
		{
			Duration = duration;
			Stage = stage;
			Plugin = plugin;
		}

		public Stages Stage { get; }
		public TimeSpan Duration { get; }
		public IReadOnlyStratumPlugin Plugin { get; }
		public bool Success => true;
	}
}
