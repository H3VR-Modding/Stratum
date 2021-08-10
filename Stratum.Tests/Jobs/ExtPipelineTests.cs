using System;
using System.Collections;
using BepInEx.Logging;
using Moq;
using Stratum.Jobs;
using Xunit;

namespace Stratum.Tests
{
	public class ExtPipelineTests
	{
		[Fact]
		private void AddNested_ReturnsSelf()
		{
			Pipeline<Empty> pipeline = new();
			Action<Pipeline<Empty>> nested = Mock.Of<Action<Pipeline<Empty>>>();

			Pipeline<Empty> ret = pipeline.AddNested(nested);

			Assert.Equal(pipeline, ret);
		}

		[Fact]
		private void AddNestedParallel_ReturnsSelf()
		{
			Pipeline<IEnumerator> pipeline = new();
			Action<Pipeline<IEnumerator>> nested = Mock.Of<Action<Pipeline<IEnumerator>>>();
			CoroutineStarter startCoroutine = Mock.Of<CoroutineStarter>();

			Pipeline<IEnumerator> ret = pipeline.AddNestedParallel(nested, startCoroutine);

			Assert.Equal(pipeline, ret);
		}

		[Fact]
		private void AddNestedSequential_ReturnsSelf()
		{
			Pipeline<IEnumerator> pipeline = new();
			Action<Pipeline<IEnumerator>> nested = Mock.Of<Action<Pipeline<IEnumerator>>>();

			Pipeline<IEnumerator> ret = pipeline.AddNestedSequential(nested);

			Assert.Equal(pipeline, ret);
		}

		[Fact]
		private void Build_CallsJobsSequentially()
		{
			Pipeline<Empty> pipeline = new();
			var jobs = new Mock<Job<Empty>>[10].Populate();
			IStage<Empty> stage = Mock.Of<IStage<Empty>>();
			ManualLogSource logger = new("fake");

			var counter = 0;
			for (var i = 0; i < jobs.Length; ++i)
			{
				ref Mock<Job<Empty>> job = ref jobs[i];
				int j = i;

				job.Setup(x => x(stage, logger))
					.Callback(() => Assert.Equal(j, counter++))
					.Returns(new Empty())
					.Verifiable();

				pipeline.AddJob(job.Object);
			}

			Job<Empty> ret = pipeline.Build();
			ret(stage, logger);

			foreach (Mock<Job<Empty>> job in jobs)
				job.Verify();
		}

		[Fact]
		private void BuildSequential_CallsJobsSequentially()
		{
			Pipeline<IEnumerator> pipeline = new();
			var jobs = new Mock<Job<IEnumerator>>[10].Populate();
			IStage<IEnumerator> stage = Mock.Of<IStage<IEnumerator>>();
			ManualLogSource logger = new("fake");

			var counter = 0;
			for (var i = 0; i < jobs.Length; ++i)
			{
				ref Mock<Job<IEnumerator>> job = ref jobs[i];
				int j = i;

				job.Setup(x => x(stage, logger))
					.Returns(() =>
					{
						IEnumerator Body()
						{
							int current = counter++;

							Assert.Equal(j, current);
							yield return null;
							Assert.Equal(j, current);
						}

						return Body();
					})
					.Verifiable();

				pipeline.AddJob(job.Object);
			}

			Job<IEnumerator> ret = pipeline.BuildSequential();

			ret(stage, logger).Enumerate();

			foreach (Mock<Job<IEnumerator>> job in jobs)
				job.Verify();
		}
	}
}
