using System.Collections.Generic;

namespace Stratum
{
	public interface IEventBatch<TPlugin> : IHasStage
	{
		int Index { get; }

		IReadOnlyList<TPlugin> Plugins { get; }
	}
}
