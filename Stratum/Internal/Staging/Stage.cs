using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;

namespace Stratum.Internal.Staging
{
	internal class Stage<TRet> : IStage<TRet>, IDisposable
	{
		private readonly IStageEssence<TRet> _essence;

		private bool _disposed;
		private ManualLogSource? _logger;
		private Dictionary<string, StageContext<TRet>>? _contexts;

		public Stage(IStageEssence<TRet> essence, int count, ManualLogSource logger)
		{
			_essence = essence;
			_logger = logger;
			_contexts = new Dictionary<string, StageContext<TRet>>(count);
		}

		private ManualLogSource Logger => _logger ?? throw new ObjectDisposedException(GetType().FullName);

		private Dictionary<string, StageContext<TRet>> Contexts => _contexts ?? throw new ObjectDisposedException(GetType().FullName);

		public Stages Variant => _essence.Variant;

		public IReadOnlyStageContext<TRet> this[string guid] => TryGet(guid) ?? throw new InvalidOperationException("The plugin" +
			$"with the GUID '{guid}' was not found.");

		public void Dispose()
		{
			if (_disposed)
				return;

			foreach (StageContext<TRet> ctx in _contexts!.Values)
				ctx.Dispose();

			_logger = null;
			_contexts = null;
			_disposed = true;
		}

		public IReadOnlyStageContext<TRet>? TryGet(string guid)
		{
			return Contexts.TryGetValue(guid, out StageContext<TRet> context) ? context : null;
		}

		public IEnumerator<IReadOnlyStageContext<TRet>> GetEnumerator()
		{
			return Contexts.Values
				.Cast<IReadOnlyStageContext<TRet>>()
				.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void RunCallback(StageContext<TRet> ctx)
		{
			ctx.Freeze();

			IReadOnlyStageContext<TRet> rctx = ctx;
			string guid = rctx.Plugin.Info.Metadata.GUID;

			Contexts.Add(guid, ctx);
		}

		public TRet Run(IStratumPlugin plugin)
		{
			Logger.LogDebug($"Loading {plugin.Info} -> {Variant}");

			return _essence.Run(new StageContext<TRet>(this, plugin), RunCallback);
		}

		public override string ToString()
		{
			return Variant.ToString();
		}
	}
}
