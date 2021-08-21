using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///		Represents a stage that has been loaded
	/// </summary>
	public interface ILoadedStage : ITimed, IHasStage
	{
		/// <summary>
		///		All the batches loaded in the stage
		/// </summary>
		IReadOnlyList<ILoadedBatch> Batches { get; }
	}
}
