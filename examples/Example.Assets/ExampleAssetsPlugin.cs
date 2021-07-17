using System.Collections;
using BepInEx;
using Stratum;
using Stratum.Extensions;
using Stratum.Jobs;

namespace Example.Assets
{
	[BepInPlugin("stratum.example.assets", "Stratum Example (Assets)", StratumRoot.Version)]
	[BepInDependency(StratumRoot.GUID, StratumRoot.Version)]
	[BepInDependency(LoadersGUID, StratumRoot.Version)]
	public class ExampleAssetsPlugin : StratumPlugin
	{
		private const string LoadersGUID = "stratum.example.loaders";

		// A loader reference can resolve to different loaders, depending on the stage.
		// Loader references are notated as <guid>::<name> e.g. stratum.example.loaders::print
		// Loader definitions are notated as <stage>/<guid>::<name> e.g. Setup/stratum.example.loaders::print
		private readonly LoaderReference _printLoader = new(LoadersGUID, "print");

		public override void OnSetup(IStageContext<Empty> ctx)
		{
			// Setup has no coroutines, so all jobs are ran in a way called "sequential".
			//
			// Sequential runs each job (e.g. asset) in order, one at a time. If one job fails, none of the jobs after it will run.
			// Think of it like items on a conveyor line, and the whole line stops if an item is defective.

			Job<Empty> assets = new AssetPipeline<Empty>(Directories.Data)
				// Will load "data/motd.txt" into Setup/stratum.example.loaders::print
				.AddAsset("motd.txt", _printLoader)
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
				.AddAsset("1.txt", _printLoader)
				// This adds another pipeline as a job, and the pipeline is ran sequentially
				.AddNestedSequential(x => x
					.WithName("Second Parts")
					.AddAsset("2.1.txt", _printLoader)
					.AddAsset("2.2.txt", _printLoader)
				)
				// Assembles the pipeline into a single, parallel job
				.BuildParallel(StartCoroutine);

			// Runs the pipeline
			// Use foreach on the IEnumerator instead of yield return, as it propagates the exception (i.e. allows it to be handled properly)
			foreach (object? item in assets(ctx.Stage, Logger))
				yield return item;
		}
	}
}
