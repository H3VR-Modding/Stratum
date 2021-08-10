using System;
using System.Collections.Generic;

namespace Stratum
{
	public sealed class BatchLoadingEventArgs : EventArgs, ILoadingBatch
	{
		internal BatchLoadingEventArgs(Stages stage, int index, IReadOnlyList<IReadOnlyStratumPlugin> plugins)
		{
			Stage = stage;
			Index = index;
			Plugins = plugins;
		}

		public Stages Stage { get; }
		public int Index { get; }
		public IReadOnlyList<IReadOnlyStratumPlugin> Plugins { get; }
	}
}
