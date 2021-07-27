using BepInEx.Logging;

namespace Stratum.Jobs
{
	/// <summary>
	///     A method that performs work within a stage
	/// </summary>
	public delegate TRet Job<TRet>(IStage<TRet> stage, ManualLogSource logger);

	/// <summary>
	///     A method that constructs a job from a pipeline
	/// </summary>
	public delegate Job<TRet> PipelineBuilder<TRet, in TPipeline>(TPipeline pipeline) where TPipeline : Pipeline<TRet, TPipeline>;
}
