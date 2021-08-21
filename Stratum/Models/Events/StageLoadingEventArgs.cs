using System;

namespace Stratum
{
	/// <summary>
	///		Event data for when a stage begins to load
	/// </summary>
	public sealed class StageLoadingEventArgs : EventArgs, IHasStage
	{
		internal StageLoadingEventArgs(Stages stage)
		{
			Stage = stage;
		}

		/// <inheritdoc cref="IHasStage.Stage"/>
		public Stages Stage { get; }
	}
}
