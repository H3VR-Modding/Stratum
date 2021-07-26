using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Stratum.Extensions;
using UnityEngine;

namespace Stratum.Jobs
{
	public sealed class AssetPipeline<TRet> : Pipeline<TRet, AssetPipeline<TRet>>
	{
		private AssetPipeline(AssetPipeline<TRet> parent, DirectoryInfo root) : base(parent)
		{
			Root = root;
		}

		public AssetPipeline(DirectoryInfo root)
		{
			Root = root;
		}

		public DirectoryInfo Root { get; }

		protected override AssetPipeline<TRet> CreateNested()
		{
			return new(this, Root);
		}

		public AssetPipeline<TRet> AddAsset(AssetReference reference)
		{
			TRet Job(IStage<TRet> stage, ManualLogSource logger)
			{
				logger.LogDebug($"{this} | Resolving {reference}");
				AssetDefinition<TRet> definition = reference.Resolve(stage, Root);

				logger.LogDebug($"{this} | Running {reference}");
				return definition.Run();
			}

			return AddJob(Job);
		}

		public AssetPipeline<TRet> AddAsset(string path, LoaderReference loader)
		{
			return AddAsset(new AssetReference(path, loader));
		}

		public AssetPipeline<TRet> AddAsset(string path, string guid, string name)
		{
			return AddAsset(new AssetReference(path, new LoaderReference(guid, name)));
		}
	}

	// Type specific methods
	public static class ExtAssetPipeline
	{
		private static PipelineBuilder<Empty, AssetPipeline<Empty>> SetupBuilder { get; } = pipeline => (stage, logger) =>
		{
			foreach (Job<Empty> job in pipeline.Jobs)
				job(stage, logger);

			return new Empty();
		};

		private static PipelineBuilder<IEnumerator, AssetPipeline<IEnumerator>> RuntimeSequentialBuilder { get; } = pipeline =>
		{
			IEnumerator Job(IStage<IEnumerator> stage, ManualLogSource logger)
			{
				foreach (Job<IEnumerator> job in pipeline.Jobs)
				foreach (var item in job(stage, logger))
					yield return item;
			}

			return Job;
		};

		private static PipelineBuilder<IEnumerator, AssetPipeline<IEnumerator>> RuntimeParallelBuilder(CoroutineStarter startCoroutine)
		{
			return pipeline =>
			{
				IEnumerator Job(IStage<IEnumerator> stage, ManualLogSource logger)
				{
					// Exceptions are thrown up the callstack (to Unity, instead of us), so we have to use some unorthodox error handling
					List<Job<IEnumerator>> jobs = pipeline.Jobs;
					var coroutines = new Coroutine[jobs.Count];
					var exceptions = new Exception?[jobs.Count];

					for (var i = 0; i < jobs.Count; ++i)
						// Swallow exception from other callstack (we throw it later)
						coroutines[i] = startCoroutine(jobs[i](stage, logger).TryCatch(e => exceptions[i] = e));

					for (var i = 0; i < jobs.Count; ++i)
					{
						yield return coroutines[i];

						// Throw exception in this callstack
						if (exceptions[i] is { } e)
							throw e;
					}
				}

				return Job;
			};
		}

		public static AssetPipeline<Empty> AddNested(this AssetPipeline<Empty> @this, Action<AssetPipeline<Empty>> nested)
		{
			return @this.AddNested(nested, SetupBuilder);
		}

		public static AssetPipeline<IEnumerator> AddNestedSequential(this AssetPipeline<IEnumerator> @this,
			Action<AssetPipeline<IEnumerator>> nested)
		{
			return @this.AddNested(nested, RuntimeSequentialBuilder);
		}

		public static AssetPipeline<IEnumerator> AddNestedParallel(this AssetPipeline<IEnumerator> @this,
			Action<AssetPipeline<IEnumerator>> nested, CoroutineStarter startCoroutine)
		{
			return @this.AddNested(nested, RuntimeParallelBuilder(startCoroutine));
		}

		public static Job<Empty> Build(this AssetPipeline<Empty> @this)
		{
			return SetupBuilder(@this);
		}

		public static Job<IEnumerator> BuildSequential(this AssetPipeline<IEnumerator> @this)
		{
			return RuntimeSequentialBuilder(@this);
		}

		public static Job<IEnumerator> BuildParallel(this AssetPipeline<IEnumerator> @this, CoroutineStarter startCoroutine)
		{
			return RuntimeParallelBuilder(startCoroutine)(@this);
		}
	}
}
