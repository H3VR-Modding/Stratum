using System.Collections.Generic;

namespace Stratum
{
	public interface IStage<TRet> : IEnumerable<IReadOnlyStageContext<TRet>>
	{
		Stages Variant { get; }

		IReadOnlyStageContext<TRet> this[string guid] { get; }

		IReadOnlyStageContext<TRet>? TryGet(string guid);
	}
}
