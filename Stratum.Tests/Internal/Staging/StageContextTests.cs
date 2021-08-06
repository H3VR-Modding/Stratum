using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using Stratum.Internal.Staging;
using Xunit;

namespace Stratum.Tests.Internal.Staging
{
	public class StageContextTests
	{
		internal static StageContext<T> Create<T>()
		{
			IStage<T> stage = Mock.Of<IStage<T>>(MockBehavior.Strict);
			IStratumPlugin plugin = Mock.Of<IStratumPlugin>(MockBehavior.Strict);

			return new StageContext<T>(stage, plugin);
		}

		[Fact]
		[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
		private void Ctor()
		{
			Create<Empty>();
		}

		[Fact]
		private void Ctor_MutableProperties_Success()
		{
			StageContext<Empty> ctx = Create<Empty>();

			IStratumPlugin plugin = ctx.Plugin;
			IDictionary<string, Loader<Empty>> loaders = ctx.Loaders;
		}

		[Fact]
		private void Freeze_ImmutableProperties_Success()
		{
			StageContext<Empty> ctx = Create<Empty>();
			IReadOnlyStageContext<Empty> rctx = ctx;

			ctx.Freeze();

			IStage<Empty> stage = rctx.Stage;
			IReadOnlyStratumPlugin plugin = rctx.Plugin;
			IReadOnlyDictionary<string, Loader<Empty>> loaders = rctx.Loaders;
		}

		[Fact]
		private void Freeze_MutableProperties_Throw()
		{
			StageContext<Empty> ctx = Create<Empty>();

			ctx.Freeze();

			Assert.Throws<ObjectFrozenException>(() => ctx.Plugin);
			Assert.Throws<ObjectFrozenException>(() => ctx.Loaders);
		}

		[Fact]
		private void Dispose_Properties_Throw()
		{
			StageContext<Empty> ctx = Create<Empty>();
			IReadOnlyStageContext<Empty> rctx = ctx;

			ctx.Dispose();

			Assert.Throws<ObjectDisposedException>(() => ctx.Loaders);
			Assert.Throws<ObjectDisposedException>(() => ctx.Plugin);
			Assert.Throws<ObjectDisposedException>(() => rctx.Plugin);
			Assert.Throws<ObjectDisposedException>(() => ctx.Loaders);
			Assert.Throws<ObjectDisposedException>(() => rctx.Loaders);
		}
	}
}
