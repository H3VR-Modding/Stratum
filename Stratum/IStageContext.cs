using System.Collections.Generic;
using Stratum.IO;

namespace Stratum
{
	public interface IReadOnlyStageContext<TRet>
	{
		IStage<TRet> Stage { get; }

		IReadOnlyStratumPlugin Plugin { get; }

		IReadOnlyDictionary<string, Loader<TRet>> Loaders { get; }

		IReadOnlyReaderCollection Readers { get; }

		IReadOnlyWriterCollection Writers { get; }
	}

	public interface IStageContext<TRet> : IReadOnlyStageContext<TRet>
	{
		new IStratumPlugin Plugin { get; }

		new IDictionary<string, Loader<TRet>> Loaders { get; }

		new IReaderCollection Readers { get; }

		new IWriterCollection Writers { get; }
	}
}
