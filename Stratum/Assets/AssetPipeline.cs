using System;
using System.Collections;
using System.IO;
using BepInEx.Logging;
using Stratum.Coroutines;
using Stratum.Extensions;
using Stratum.Jobs;
using UnityEngine;

namespace Stratum.Assets
{
	public sealed class AssetPipeline<TRet> : Pipeline<TRet, AssetPipeline<TRet>>
	{
		public DirectoryInfo Root { get; }

		private AssetPipeline(AssetPipeline<TRet> parent, DirectoryInfo root) : base(parent) => Root = root;

		public AssetPipeline(DirectoryInfo root) => Root = root;

		protected override AssetPipeline<TRet> CreateNested() => new(this, Root);

		public AssetPipeline<TRet> AddAsset(AssetReference reference)
		{
			TRet Job(IStage<TRet> stage, ManualLogSource logger)
			{
				logger.LogDebug($"{this} | Resolving {reference}");
				var definition = reference.Resolve(stage, Root);

				logger.LogDebug($"{this} | Resolved and running {definition}");
				return definition.Run();
			}

			return AddJob(Job);
		}

		public AssetPipeline<TRet> AddAsset(string path, LoaderReference loader) =>
			AddAsset(new(path, loader));

		public AssetPipeline<TRet> AddAsset(string path, string guid, string name) =>
			AddAsset(new(path, new(guid, name)));
	}

	// Type specific methods
	public static class ExtAssetPipeline
	{
		private static PipelineBuilder<Empty, AssetPipeline<Empty>> SetupBuilder { get; } = pipeline => (stage, logger) =>
		{
			foreach (var job in pipeline.Jobs)
				job(stage, logger);

			return new Empty();
		};

		private static PipelineBuilder<IEnumerator, AssetPipeline<IEnumerator>> RuntimeSequentialBuilder { get; } = pipeline =>
		{
			IEnumerator Job(IStage<IEnumerator> stage, ManualLogSource logger)
			{
				foreach (var job in pipeline.Jobs)
				foreach (var item in job(stage, logger))
					yield return item;
			}

			return Job;
		};

		private static PipelineBuilder<IEnumerator, AssetPipeline<IEnumerator>> RuntimeParallelBuilder(CoroutineStarter startCoroutine) => pipeline =>
		{
			IEnumerator Job(IStage<IEnumerator> stage, ManualLogSource logger)
			{
				// Exceptions are thrown up the callstack (to Unity, instead of us), so we have to use some unorthodox error handling
				var jobs = pipeline.Jobs;
				var coroutines = new Coroutine[jobs.Count];
				var exceptions = new Exception?[jobs.Count];

				for (var i = 0; i < jobs.Count; ++i)
					// Swallow exception from other callstack (we throw it later)
					coroutines[i] = startCoroutine(jobs[i](stage, logger).TryCatch(e => exceptions[i] = e));

				for (var i = 0; i < jobs.Count; ++i)
				{
					yield return coroutines[i];

					var e = exceptions[i];
					if (e is null)
						continue;

					// Throw exception in this callstack
					throw e;
				}
			}

			return Job;
		};

		public static AssetPipeline<Empty> AddNested(this AssetPipeline<Empty> @this, Action<AssetPipeline<Empty>> nested) =>
			@this.AddNested(nested, SetupBuilder);

		public static AssetPipeline<IEnumerator> AddNestedSequential(this AssetPipeline<IEnumerator> @this, Action<AssetPipeline<IEnumerator>> nested) =>
			@this.AddNested(nested, RuntimeSequentialBuilder);

		public static AssetPipeline<IEnumerator> AddNestedParallel(this AssetPipeline<IEnumerator> @this, Action<AssetPipeline<IEnumerator>> nested,
			CoroutineStarter startCoroutine) => @this.AddNested(nested, RuntimeParallelBuilder(startCoroutine));

		public static Job<Empty> Build(this AssetPipeline<Empty> @this) => SetupBuilder(@this);

		public static Job<IEnumerator> BuildSequential(this AssetPipeline<IEnumerator> @this) => RuntimeSequentialBuilder(@this);

		public static Job<IEnumerator> BuildParallel(this AssetPipeline<IEnumerator> @this, CoroutineStarter startCoroutine) =>
			RuntimeParallelBuilder(startCoroutine)(@this);
	}
}
