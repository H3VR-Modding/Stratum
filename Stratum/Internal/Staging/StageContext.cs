using System;
using System.Collections.Generic;
using Stratum.Internal.IO;
using Stratum.IO;

namespace Stratum.Internal.Staging
{
	internal sealed class StageContext<TRet> : IStageContext<TRet>, IFreezable, IDisposable
	{
		private IStage<TRet>? _stage;
		private IStratumPlugin? _plugin;
		private LoaderDictionary<TRet>? _loaders = new();
		private ReaderCollection? _readers = new();
		private WriterCollection? _writers = new();

		private bool _frozen;

		public IStage<TRet> Stage => DisposeRet(_stage);

		public IStratumPlugin Plugin => FreezeRet(_plugin);
		IReadOnlyStratumPlugin IReadOnlyStageContext<TRet>.Plugin => DisposeRet(_plugin);

		public IDictionary<string, Loader<TRet>> Loaders => FreezeRet(_loaders);
		IReadOnlyDictionary<string, Loader<TRet>> IReadOnlyStageContext<TRet>.Loaders => DisposeRet(_loaders);

		public IReaderCollection Readers => FreezeRet(_readers);
		IReadOnlyReaderCollection IReadOnlyStageContext<TRet>.Readers => DisposeRet(_readers);

		public IWriterCollection Writers => FreezeRet(_writers);
		IReadOnlyWriterCollection IReadOnlyStageContext<TRet>.Writers => DisposeRet(_writers);

		public StageContext(IStage<TRet> stage, IStratumPlugin plugin)
		{
			_stage = stage;
			_plugin = plugin;
		}

		private T FreezeRet<T>(T? value) => !_frozen ? DisposeRet(value) : throw new ObjectFrozenException(GetType().FullName);

		private T DisposeRet<T>(T? value) => value ?? throw new ObjectDisposedException(GetType().FullName);

		public void Freeze()
		{
			_loaders?.Freeze();
			_readers?.Freeze();
			_writers?.Freeze();

			_frozen = true;
		}

		public void Dispose()
		{
			_loaders?.Dispose();
			_readers?.Dispose();
			_writers?.Dispose();

			_stage = null;
			_plugin = null;
			_loaders = null;
			_readers = null;
			_writers = null;
		}
	}
}
