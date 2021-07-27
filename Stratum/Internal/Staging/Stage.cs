using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;

namespace Stratum.Internal.Staging
{
	internal abstract class Stage<TRet> : IStage<TRet>, IDisposable
	{
		private Dictionary<string, StageContext<TRet>>? _contexts;

		protected Stage(int count, ManualLogSource logger)
		{
			_contexts = new Dictionary<string, StageContext<TRet>>(count);

			Logger = logger;
		}

		private Dictionary<string, StageContext<TRet>> Contexts =>
			_contexts ?? throw new ObjectDisposedException(GetType().FullName);

		protected ManualLogSource Logger { get; }

		public abstract Stages Variant { get; }

		public IReadOnlyStageContext<TRet> this[string guid] => TryGet(guid) ?? throw new InvalidOperationException("The plugin" +
			$"with the GUID '{guid}' was not found.");

		public void Dispose()
		{
			if (_contexts == null)
				return;

			foreach (StageContext<TRet> ctx in _contexts.Values)
				ctx.Dispose();

			_contexts = null;
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

		protected void EndRun(StageContext<TRet> ctx)
		{
			ctx.Freeze();

			IReadOnlyStageContext<TRet> rctx = ctx;
			string guid = rctx.Plugin.Info.Metadata.GUID;

			Contexts.Add(guid, ctx);
		}

		protected abstract TRet BeginRun(StageContext<TRet> ctx);

		public TRet Run(IStratumPlugin plugin)
		{
			Logger.LogDebug($"Loading {plugin.Info} -> {Variant}");

			return BeginRun(new StageContext<TRet>(this, plugin));
		}

		public override string ToString()
		{
			return Variant.ToString();
		}
	}
}
