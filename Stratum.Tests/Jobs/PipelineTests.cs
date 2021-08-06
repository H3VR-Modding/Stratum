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
		public static Job<Empty> EmptyJob => (_, _) => new Empty();
		public static Action<Pipeline<Empty>> EmptyNested => _ => { };
		public static PipelineBuilder<Empty, Pipeline<Empty>> EmptyBuilder => _ => EmptyJob;

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

			Pipeline<Empty> ret = pipeline.AddJob(EmptyJob);

			Assert.Equal(pipeline, ret);
		}

		[Fact]
		private void AddJob_AddsJobToList()
		{
			Pipeline<Empty> pipeline = new();
			Job<Empty> job = EmptyJob;

			pipeline.AddJob(job);

			Assert.Equal(new[] {job}, pipeline.Jobs);
		}

		[Fact]
		private void AddNested_ReturnsSelf()
		{
			Pipeline<Empty> pipeline = new();

			Pipeline<Empty> ret = pipeline.AddNested(EmptyNested, EmptyBuilder);

			Assert.Equal(pipeline, ret);
		}

		[Fact]
		private void AddNested_NestedIsParent()
		{
			Pipeline<Empty> pipeline = new();
			Pipeline<Empty>? nested = null;

			pipeline.AddNested(x => nested = x, EmptyBuilder);

			Assert.NotNull(nested);
			Assert.Equal(pipeline, nested!.Parent);
		}

		[Fact]
		private void AddNested_PipelineIsPassedToBuilder()
		{
			Pipeline<Empty> pipeline = new();
			Pipeline<Empty>? nested = null;
			Pipeline<Empty>? built = null;

			pipeline.AddNested(x => nested = x, x =>
			{
				built = x;

				return EmptyJob;
			});

			Assert.NotNull(nested);
			Assert.NotNull(built);
			Assert.Equal(nested, built);
		}

		[Fact]
		private void AddNested_AddsJobToList()
		{
			Pipeline<Empty> pipeline = new();
			IStage<Empty> stage = Mock.Of<IStage<Empty>>(MockBehavior.Strict);
			ManualLogSource logger = new("fake");
			var ran = false;

			pipeline.AddNested(x => x.AddJob((_, _) =>
			{
				ran = true;

				return new Empty();
			}));

			Job<Empty> single = Assert.Single(pipeline.Jobs)!;
			single(stage, logger);

			Assert.True(ran);
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
