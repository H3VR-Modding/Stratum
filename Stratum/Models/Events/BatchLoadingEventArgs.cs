using System;
using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///		Event data for when a batch of plugins begins to load
	/// </summary>
	public sealed class BatchLoadingEventArgs : EventArgs, IEventBatch<IReadOnlyStratumPlugin>
	{
		internal BatchLoadingEventArgs(Stages stage, int index, IReadOnlyList<IReadOnlyStratumPlugin> plugins)
		{
			Stage = stage;
			Generation = index;
			Plugins = plugins;
		}

		/// <inheritdoc cref="IHasStage.Stage"/>
		public Stages Stage { get; }

		/// <inheritdoc cref="IEventBatch{TPlugin}.Generation"/>
		public int Generation { get; }

		/// <inheritdoc cref="IEventBatch{TPlugin}.Plugins"/>
		public IReadOnlyList<IReadOnlyStratumPlugin> Plugins { get; }
	}
}
