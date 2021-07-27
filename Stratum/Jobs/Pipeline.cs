using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using Stratum.Extensions;
using UnityEngine;

namespace Stratum.Jobs
{
	/// <summary>
	///     A collection of jobs to be ran
	/// </summary>
	public abstract class Pipeline<TRet, TSelf> where TSelf : Pipeline<TRet, TSelf>
	{
		/// <summary>
		///     Constructs an instance of <see cref="Pipeline{TRet,TSelf}" />
		/// </summary>
		/// <param name="parent">The pipeline that will execute this pipeline</param>
		protected Pipeline(TSelf parent)
		{
			Parent = parent;
		}

		/// <summary>
		///     Constructs an instance of <see cref="Pipeline{TRet,TSelf}" />
		/// </summary>
		public Pipeline() { }

		/// <summary>
		///     The pipeline that will execute this pipeline
		/// </summary>
		public TSelf? Parent { get; }

		/// <summary>
		///     The jobs to execute
		/// </summary>
		public List<Job<TRet>> Jobs { get; } = new();

		/// <summary>
		///     The name to display in logs
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		///     Creates a pipeline with its parent as this pipeline
		/// </summary>
		protected abstract TSelf CreateNested();

		/// <summary>
		///     Sets <see cref="Name" />
		/// </summary>
		/// <param name="name">The name to display in logs</param>
		public TSelf WithName(string? name)
		{
			Name = name;
			return (TSelf) this;
		}

		/// <summary>
		///     Adds a job to <see cref="Jobs" />
		/// </summary>
		/// <param name="job">The job to add</param>
		public TSelf AddJob(Job<TRet> job)
		{
			Jobs.Add(job);
			return (TSelf) this;
		}

		/// <summary>
		///     Adds a pipeline, which will be executed as a job
		/// </summary>
		/// <param name="nested">Modifiers to apply to the pipeline</param>
		/// <param name="builder">How the pipeline should be ran</param>
		public TSelf AddNested(Action<TSelf> nested, PipelineBuilder<TRet, TSelf> builder)
		{
			TSelf subpipe = CreateNested();
			nested(subpipe);
			Job<TRet> job = builder(subpipe);

			return AddJob(job);
		}

		/// <summary>
		///     Displays the full hierarchy of this pipeline
		/// </summary>
		public override string ToString()
		{
			const string unnamed = "<unnamed>";
			StringBuilder builder = new();

			// Path-like structure to determine the callstack of the pipeline.
			TSelf? parent = Parent;
			while (parent != null)
			{
				builder
					.Insert(0, '/')
					.Insert(0, parent.Name ?? unnamed);
				parent = parent.Parent;
			}

			builder.Append(Name ?? unnamed);

			return builder.ToString();
		}
	}

	/// <summary>
	///     A collection of jobs to be ran
	/// </summary>
	public sealed class Pipeline<TRet> : Pipeline<TRet, Pipeline<TRet>>
	{
		private Pipeline(Pipeline<TRet> parent) : base(parent) { }

		/// <inheritdoc cref="Pipeline{TRet,TSelf}.CreateNested" />
		protected override Pipeline<TRet> CreateNested()
		{
			return new(this);
		}
	}

	/// <summary>
	///     Type specific methods for <see cref="Pipeline{TRet,TSelf}" />
	/// </summary>
	public static class ExtPipeline
	{
		private static Job<Empty> SetupBuilder<TSelf>(TSelf pipeline) where TSelf : Pipeline<Empty, TSelf>
		{
			Empty Job(IStage<Empty> stage, ManualLogSource logger)
			{
				foreach (Job<Empty> job in pipeline.Jobs)
					job(stage, logger);

				return new Empty();
			}

			return Job;
		}

		private static Job<IEnumerator> RuntimeSequentialBuilder<TSelf>(TSelf pipeline) where TSelf : Pipeline<IEnumerator, TSelf>
		{
			IEnumerator Job(IStage<IEnumerator> stage, ManualLogSource logger)
			{
				foreach (Job<IEnumerator> job in pipeline.Jobs)
				foreach (var item in job(stage, logger))
					yield return item;
			}

			return Job;
		}

		private static Job<IEnumerator> RuntimeParallelBuilder<TSelf>(TSelf pipeline, CoroutineStarter startCoroutine)
			where TSelf : Pipeline<IEnumerator, TSelf>
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
		}

		/// <summary>
		///     Adds a nested pipeline, which is built using the default setup builder
		/// </summary>
		/// <param name="this"></param>
		/// <param name="nested">Modifiers to apply to the pipeline</param>
		public static TSelf AddNested<TSelf>(this TSelf @this, Action<TSelf> nested) where TSelf : Pipeline<Empty, TSelf>
		{
			return @this.AddNested(nested, SetupBuilder);
		}

		/// <summary>
		///     Adds a nested pipeline, which is built using the sequential runtime builder
		/// </summary>
		/// <param name="this"></param>
		/// <param name="nested">Modifiers to apply to the pipeline</param>
		public static TSelf AddNestedSequential<TSelf>(this TSelf @this, Action<TSelf> nested) where TSelf : Pipeline<IEnumerator, TSelf>
		{
			return @this.AddNested(nested, RuntimeSequentialBuilder);
		}

		/// <summary>
		///     Adds a nested pipeline, which is built using the parallel runtime builder
		/// </summary>
		/// <param name="this"></param>
		/// <param name="nested">Modifiers to apply to the pipeline</param>
		/// <param name="startCoroutine">The method to start Unity coroutines with</param>
		public static TSelf AddNestedParallel<TSelf>(this TSelf @this, Action<TSelf> nested, CoroutineStarter startCoroutine)
			where TSelf : Pipeline<IEnumerator, TSelf>
		{
			return @this.AddNested(nested, pipeline => RuntimeParallelBuilder(pipeline, startCoroutine));
		}

		/// <summary>
		///     Creates a job to run the pipeline using the default setup builder
		/// </summary>
		/// <param name="this"></param>
		public static Job<Empty> Build<TSelf>(this TSelf @this) where TSelf : Pipeline<Empty, TSelf>
		{
			return SetupBuilder(@this);
		}

		/// <summary>
		///     Creates a job to run the pipeline using the sequential runtime builder
		/// </summary>
		/// <param name="this"></param>
		public static Job<IEnumerator> BuildSequential<TSelf>(this TSelf @this) where TSelf : Pipeline<IEnumerator, TSelf>
		{
			return RuntimeSequentialBuilder(@this);
		}

		/// <summary>
		///     Creates a job to run the pipeline using the parallel runtime builder
		/// </summary>
		/// <param name="this"></param>
		/// <param name="startCoroutine">The method to start Unity coroutines with</param>
		public static Job<IEnumerator> BuildParallel<TSelf>(this TSelf @this, CoroutineStarter startCoroutine)
			where TSelf : Pipeline<IEnumerator, TSelf>
		{
			return RuntimeParallelBuilder(@this, startCoroutine);
		}
	}
}
