using System.Collections.Generic;

namespace Stratum
{
	public interface IStage<TRet> : IEnumerable<IReadOnlyStageContext<TRet>>
	{
		Stages StageType { get; }

		IReadOnlyStageContext<TRet>? TryGet(string guid);

		IReadOnlyStageContext<TRet> this[string guid] { get; }
	}
}
