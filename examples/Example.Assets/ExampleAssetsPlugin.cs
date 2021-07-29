using System.Collections;
using BepInEx;
using Stratum;
using Stratum.Extensions;
using Stratum.Jobs;

namespace Example.Assets
{
	[BepInPlugin("stratum.example.assets", "Stratum Example (Assets)", StratumRoot.Version)]
	[BepInDependency(LoadersPlugin, StratumRoot.Version)]
	public class ExampleAssetsPlugin : StratumPlugin
	{
		private const string LoadersPlugin = "stratum.example.loaders";
		private const string PrintLoader = "print";

		public override void OnSetup(IStageContext<Empty> ctx)
		{
			// Setup has no coroutines, so all jobs are ran in a way called "sequential".
			//
			// Sequential runs each job (f.e. asset) in order, one at a time. If one job fails, none of the jobs after it will run.
			// Think of it like items on a conveyor line, and the whole line stops if an item is defective.

			Job<Empty> assets = new AssetPipeline<Empty>(Directories.Data)
				// Will load "data/motd.txt" into Setup/stratum.example.loaders::print
				.AddAsset(LoadersPlugin, PrintLoader, "motd.txt")
				// Assembles the pipeline into a single, sequential job
				.Build();

			// Runs the pipeline
			assets(ctx.Stage, Logger);
		}

		public override IEnumerator OnRuntime(IStageContext<IEnumerator> ctx)
		{
			// Runtime does have coroutines, so it has the option of running jobs in parallel.
			//
			// Parallel runs each job at the same time. If one job fails, it doesn't matter. All the jobs are already running.
			// Think of it like items flung by a catapult, and some of the items might miss the target.

			Job<IEnumerator> assets = new AssetPipeline<IEnumerator>(Directories.Resources)
				// Will load "resources/1.txt" into Runtime/stratum.example.loaders::print
				.AddAsset(LoadersPlugin, PrintLoader, "1.txt")
				// This adds another pipeline as a job, and the pipeline is ran sequentially
				.AddNestedSequential(x => x
					.WithName("Second Parts")
					.AddAsset(LoadersPlugin, PrintLoader, "2.1.txt")
					.AddAsset(LoadersPlugin, PrintLoader, "2.2.txt")
				)
				// Assembles the pipeline into a single, parallel job
				.BuildParallel(StartCoroutine);

			// Runs the pipeline
			return assets(ctx.Stage, Logger);
		}
	}
}
