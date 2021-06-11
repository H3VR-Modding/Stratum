using BepInEx.Logging;

namespace Stratum.Jobs
{
	public delegate TRet Job<TRet>(IStage<TRet> stage, ManualLogSource logger);

	public delegate Job<TRet> PipelineBuilder<TRet, in TPipeline>(TPipeline pipeline) where TPipeline : Pipeline<TRet, TPipeline>;
}
