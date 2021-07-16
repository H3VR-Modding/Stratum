using System.Collections.Generic;

namespace Stratum
{
	public interface IReadOnlyStageContext<TRet>
	{
		IStage<TRet> Stage { get; }

		IReadOnlyStratumPlugin Plugin { get; }

		IReadOnlyDictionary<string, Loader<TRet>> Loaders { get; }
	}

	public interface IStageContext<TRet> : IReadOnlyStageContext<TRet>
	{
		new IStratumPlugin Plugin { get; }

		new IDictionary<string, Loader<TRet>> Loaders { get; }
	}
}
