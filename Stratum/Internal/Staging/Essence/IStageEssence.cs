using System;

namespace Stratum.Internal.Staging
{
	internal interface IStageEssence<TRet>
	{
		Stages Variant { get; }

		TRet Run(StageContext<TRet> ctx, Action callback);
	}
}
