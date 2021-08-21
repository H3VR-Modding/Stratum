using System;
using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///		Event data for when a stage finishes loading
	/// </summary>
	public sealed class StageLoadedEventArgs : EventArgs, ILoadedStage
	{
		internal StageLoadedEventArgs(TimeSpan duration, Stages stage, IReadOnlyList<ILoadedBatch> batches)
		{
			Duration = duration;
			Stage = stage;
			Batches = batches;
		}

		/// <inheritdoc cref="ITimed.Duration"/>
		public TimeSpan Duration { get; }

		/// <inheritdoc cref="IHasStage.Stage"/>
		public Stages Stage { get; }

		/// <inheritdoc cref="ILoadedStage.Batches"/>
		public IReadOnlyList<ILoadedBatch> Batches { get; }
	}
}
