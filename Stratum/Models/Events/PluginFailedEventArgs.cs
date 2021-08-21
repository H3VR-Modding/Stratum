using System;
using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///     Event data for when a plugin errors while loading
	/// </summary>
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

		/// <inheritdoc cref="ITimed.Duration" />
		public TimeSpan Duration { get; }

		/// <inheritdoc cref="IHasStage.Stage" />
		public Stages Stage { get; }

		/// <inheritdoc cref="ILoadedPlugin.Plugin" />
		public IReadOnlyStratumPlugin Plugin { get; }

		/// <inheritdoc cref="ILoadedPlugin.Success" />
		public bool Success => false;

		/// <summary>
		///     The plugins that depend on the failed plugin
		/// </summary>
		public IReadOnlyList<IReadOnlyStratumPlugin> Dependents { get; }

		/// <summary>
		///     The exception thrown by the plugin
		/// </summary>
		public Exception Exception { get; }
	}
}
