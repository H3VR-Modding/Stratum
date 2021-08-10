using System;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;
using Moq;
using Stratum.Jobs;
using Xunit;

namespace Stratum.Tests
{
	public class PipelineTests
	{
		[Fact]
		[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
		private void Ctor_NoArgs()
		{
			new Pipeline<Empty>();
		}

		[Fact]
		private void AddJob_ReturnsSelf()
		{
			Pipeline<Empty> pipeline = new();
			Job<Empty> job = Mock.Of<Job<Empty>>();

			Pipeline<Empty> ret = pipeline.AddJob(job);

			Assert.Equal(pipeline, ret);
		}

		[Fact]
		private void AddJob_AddsJobToList()
		{
			Pipeline<Empty> pipeline = new();
			Job<Empty> job = Mock.Of<Job<Empty>>();

			pipeline.AddJob(job);

			Assert.Equal(new[] {job}, pipeline.Jobs);
		}

		[Fact]
		private void AddNested_ReturnsSelf()
		{
			Pipeline<Empty> pipeline = new();
			Action<Pipeline<Empty>> nested = Mock.Of<Action<Pipeline<Empty>>>();
			PipelineBuilder<Empty, Pipeline<Empty>> builder = Mock.Of<PipelineBuilder<Empty, Pipeline<Empty>>>();

			Pipeline<Empty> ret = pipeline.AddNested(nested, builder);

			Assert.Equal(pipeline, ret);
		}

		[Fact]
		private void AddNested_NestedIsParent()
		{
			Pipeline<Empty> pipeline = new();
			Mock<Action<Pipeline<Empty>>> nested = new();
			PipelineBuilder<Empty, Pipeline<Empty>> builder = Mock.Of<PipelineBuilder<Empty, Pipeline<Empty>>>();

			pipeline.AddNested(nested.Object, builder);

			nested.Verify(x => x(It.Is<Pipeline<Empty>>(y => pipeline == y.Parent)));
		}

		[Fact]
		private void AddNested_PipelineIsPassedToBuilder()
		{
			Pipeline<Empty> pipeline = new();
			Mock<Action<Pipeline<Empty>>> nested = new();
			Mock<PipelineBuilder<Empty, Pipeline<Empty>>> builder = new();
			Pipeline<Empty>? passed = null;

			nested.Setup(x => x(It.IsAny<Pipeline<Empty>>()))
				.Callback((Pipeline<Empty> x) => passed = x)
				.Verifiable();
			builder.Setup(x => x(It.Is<Pipeline<Empty>>(y => y == passed)))
				.Returns(Mock.Of<Job<Empty>>())
				.Verifiable();

			pipeline.AddNested(nested.Object, builder.Object);

			nested.Verify();
			builder.Verify();
		}

		[Fact]
		private void AddNested_AddsJobToList()
		{
			Pipeline<Empty> pipeline = new();
			IStage<Empty> stage = Mock.Of<IStage<Empty>>();
			ManualLogSource logger = new("fake");
			Mock<Job<Empty>> job = new();

			job.Setup(x => x(It.IsAny<IStage<Empty>>(), It.IsAny<ManualLogSource>()))
				.Returns(new Empty())
				.Verifiable();

			pipeline.AddNested(x => x.AddJob(job.Object));
			Job<Empty> single = Assert.Single(pipeline.Jobs)!;
			single(stage, logger);

			job.Verify();
		}

		[Fact]
		private void WithName_ReturnsSelf()
		{
			Pipeline<Empty> pipeline = new();

			Pipeline<Empty> ret = pipeline.WithName(null);

			Assert.Equal(pipeline, ret);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("name")]
		private void WithName_NameChanged(string? name)
		{
			Pipeline<Empty> pipeline = new();

			pipeline.WithName(name);

			Assert.Equal(name, pipeline.Name);
		}
	}
}
