using System;

namespace Stratum
{
	/// <summary>
	///		Event data for when Stratum finishes loading
	/// </summary>
	public sealed class LoadedStratumEventArgs : EventArgs, ITimed
	{
		internal LoadedStratumEventArgs(TimeSpan duration)
		{
			Duration = duration;
		}

		/// <inheritdoc cref="ITimed.Duration"/>
		public TimeSpan Duration { get; }
	}
}
