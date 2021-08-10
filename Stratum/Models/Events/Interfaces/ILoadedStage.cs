using System.Collections.Generic;

namespace Stratum
{
	public interface ILoadedStage : ITimed, IHasStage
	{
		IReadOnlyList<ILoadedBatch> Batches { get; }
	}
}
