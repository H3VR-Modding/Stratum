using System;

namespace Stratum
{
	public sealed class LoadedStratumEventArgs : EventArgs, ITimed
	{
		internal LoadedStratumEventArgs(TimeSpan duration)
		{
			Duration = duration;
		}

		public TimeSpan Duration { get; }
	}
}
