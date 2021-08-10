using System;
using System.Collections.Generic;

namespace Stratum
{
	public sealed class BatchLoadedEventArgs : EventArgs, ILoadedBatch
	{
		internal BatchLoadedEventArgs(TimeSpan duration, Stages stages, int index, IReadOnlyList<ILoadedPlugin> plugins)
		{
			Duration = duration;
			Stage = stages;
			Index = index;
			Plugins = plugins;
		}

		public TimeSpan Duration { get; }
		public Stages Stage { get; }
		public int Index { get; }
		public IReadOnlyList<ILoadedPlugin> Plugins { get; }
	}
}
