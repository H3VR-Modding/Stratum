using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Stratum.Internal.Staging
{
	internal class Stage<TRet> : IStage<TRet>, IDisposable
	{
		private readonly IStageEssence<TRet> _essence;
		private Dictionary<string, StageContext<TRet>>? _contexts;

		private bool _disposed;

		public Stage(IStageEssence<TRet> essence, int count)
		{
			_essence = essence;
			_contexts = new Dictionary<string, StageContext<TRet>>(count);
		}

		private Dictionary<string, StageContext<TRet>> Contexts =>
			_contexts ?? throw new ObjectDisposedException(GetType().FullName);

		public Stages Variant => _essence.Variant;

		public IReadOnlyStageContext<TRet> this[string guid] => TryGet(guid) ?? throw new InvalidOperationException(
			"The plugin" +
			$"with the GUID '{guid}' was not found.");

		public void Dispose()
		{
			if (_disposed)
				return;

			foreach (StageContext<TRet> ctx in _contexts!.Values)
				ctx.Dispose();

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

		private Action RunCallback(StageContext<TRet> ctx)
		{
			return () =>
			{
				ctx.Freeze();

				IReadOnlyStageContext<TRet> rctx = ctx;
				string guid = rctx.Plugin.Info.Metadata.GUID;

				Contexts.Add(guid, ctx);
			};
		}

		public TRet Run(IStratumPlugin plugin)
		{
			StageContext<TRet> ctx = new(this, plugin);

			return _essence.Run(ctx, RunCallback(ctx));
		}

		public override string ToString()
		{
			return Variant.ToString();
		}
	}
}
