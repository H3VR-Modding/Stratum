using System;

namespace Stratum
{
	/// <summary>
	///		Event data for when a plugin finishes loading
	/// </summary>
	public sealed class PluginLoadedEventArgs : EventArgs, ILoadedPlugin
	{
		internal PluginLoadedEventArgs(TimeSpan duration, Stages stage, IStratumPlugin plugin)
		{
			Duration = duration;
			Stage = stage;
			Plugin = plugin;
		}

		/// <inheritdoc cref="IHasStage.Stage"/>
		public Stages Stage { get; }

		/// <inheritdoc cref="ITimed.Duration"/>
		public TimeSpan Duration { get; }

		/// <inheritdoc cref="ILoadedPlugin.Plugin"/>
		public IReadOnlyStratumPlugin Plugin { get; }

		/// <inheritdoc cref="ILoadedPlugin.Success"/>
		public bool Success => true;
	}
}
