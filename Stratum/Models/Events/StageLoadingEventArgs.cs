using System;

namespace Stratum
{
	public sealed class StageLoadingEventArgs : EventArgs, IHasStage
	{
		internal StageLoadingEventArgs(Stages stage)
		{
			Stage = stage;
		}

		public Stages Stage { get; }
	}
}
