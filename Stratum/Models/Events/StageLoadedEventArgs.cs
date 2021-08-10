using System;
using System.Collections.Generic;

namespace Stratum
{
	public sealed class StageLoadedEventArgs : EventArgs, ILoadedStage
	{
		internal StageLoadedEventArgs(TimeSpan duration, Stages stage, IReadOnlyList<ILoadedBatch> batches)
		{
			Duration = duration;
			Stage = stage;
			Batches = batches;
		}

		public TimeSpan Duration { get; }
		public Stages Stage { get; }
		public IReadOnlyList<ILoadedBatch> Batches { get; }
	}
}
