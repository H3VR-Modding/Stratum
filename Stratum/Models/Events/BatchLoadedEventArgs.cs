using System;
using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///		Event data for when a batch of plugins finishes loading
	/// </summary>
	public sealed class BatchLoadedEventArgs : EventArgs, ILoadedBatch
	{
		internal BatchLoadedEventArgs(TimeSpan duration, Stages stage, int generation, IReadOnlyList<ILoadedPlugin> plugins)
		{
			Duration = duration;
			Stage = stage;
			Generation = generation;
			Plugins = plugins;
		}

		/// <inheritdoc cref="ITimed.Duration"/>
		public TimeSpan Duration { get; }

		/// <inheritdoc cref="IHasStage.Stage"/>
		public Stages Stage { get; }

		/// <inheritdoc cref="IEventBatch{TPlugin}.Generation"/>
		public int Generation { get; }

		/// <inheritdoc cref="IEventBatch{TPlugin}.Plugins"/>
		public IReadOnlyList<ILoadedPlugin> Plugins { get; }
	}
}
