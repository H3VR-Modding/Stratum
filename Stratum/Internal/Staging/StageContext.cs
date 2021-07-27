using System;
using System.Collections.Generic;
using Stratum.Internal.IO;

namespace Stratum.Internal.Staging
{
	internal sealed class StageContext<TRet> : IStageContext<TRet>, IFreezable, IDisposable
	{
		private bool _frozen;
		private LoaderDictionary<TRet>? _loaders = new();
		private IStratumPlugin? _plugin;
		private IStage<TRet>? _stage;

		public StageContext(IStage<TRet> stage, IStratumPlugin plugin)
		{
			_stage = stage;
			_plugin = plugin;
		}

		public IStage<TRet> Stage => DisposeRet(_stage);

		public IStratumPlugin Plugin => FreezeRet(_plugin);
		IReadOnlyStratumPlugin IReadOnlyStageContext<TRet>.Plugin => DisposeRet(_plugin);

		public IDictionary<string, Loader<TRet>> Loaders => FreezeRet(_loaders);
		IReadOnlyDictionary<string, Loader<TRet>> IReadOnlyStageContext<TRet>.Loaders => DisposeRet(_loaders);

		public void Dispose()
		{
			_loaders?.Dispose();

			_stage = null;
			_plugin = null;
			_loaders = null;
		}

		public void Freeze()
		{
			_loaders?.Freeze();

			_frozen = true;
		}

		private T FreezeRet<T>(T? value)
		{
			return !_frozen ? DisposeRet(value) : throw new ObjectFrozenException(GetType().FullName);
		}

		private T DisposeRet<T>(T? value)
		{
			return value ?? throw new ObjectDisposedException(GetType().FullName);
		}
	}
}
