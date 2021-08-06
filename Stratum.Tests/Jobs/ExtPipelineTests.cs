using System;
using System.Collections;
using System.Linq;
using BepInEx.Logging;
using Moq;
using Stratum.Extensions;
using Stratum.Jobs;
using UnityEngine;
using Xunit;

namespace Stratum.Tests
{
	public class ExtPipelineTests
	{
		private static Action<Pipeline<IEnumerator>> EmptyNestedParallel => _ => { };

		[Fact]
		private void AddNested_ReturnsSelf()
		{
			Pipeline<Empty> pipeline = new();

			Pipeline<Empty> ret = pipeline.AddNested(PipelineTests.EmptyNested);

			Assert.Equal(pipeline, ret);
		}

		[Fact]
		private void AddNestedParallel_ReturnsSelf()
		{
			Pipeline<IEnumerator> pipeline = new();

			Pipeline<IEnumerator> ret = pipeline.AddNestedParallel(EmptyNestedParallel, _ => null!);

			Assert.Equal(pipeline, ret);
		}

		[Fact]
		private void AddNestedSequential_ReturnsSelf()
		{
			Pipeline<IEnumerator> pipeline = new();

			Pipeline<IEnumerator> ret = pipeline.AddNestedSequential(EmptyNestedParallel);

			Assert.Equal(pipeline, ret);
		}

		[Fact]
		private void Build_CallsJobsSequentially()
		{
			var counter = 0;
			Pipeline<Empty> pipeline = new();
			IStage<Empty> stage = Mock.Of<IStage<Empty>>(MockBehavior.Strict);
			ManualLogSource logger = new("fake");

			for (var i = 0; i < 10; ++i)
			{
				int j = i;
				pipeline.AddJob((_, _) =>
				{
					Assert.Equal(j, counter++);

					return new Empty();
				});
			}

			Job<Empty> ret = pipeline.Build();
			ret(stage, logger);
		}

		[Fact]
		private void BuildSequential_CallsJobsSequentially()
		{
			var counter = 0;
			Pipeline<IEnumerator> pipeline = new();
			IStage<IEnumerator> stage = Mock.Of<IStage<IEnumerator>>(MockBehavior.Strict);
			ManualLogSource logger = new("fake");

			for (var i = 0; i < 10; ++i)
			{
				int j = i;
				pipeline.AddJob((_, _) =>
				{
					IEnumerator Body()
					{
						int current = counter++;

						Assert.Equal(j, current);

						yield return null;

						Assert.Equal(j, current);
					}

					return Body();
				});
			}

			Job<IEnumerator> ret = pipeline.BuildSequential();

			foreach (object? item in ret(stage, logger)) { }
		}
	}
}
