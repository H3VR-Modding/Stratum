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

		public void Dispose()
		{
			if (_contexts is null)
				return;

			foreach (var ctx in _contexts.Values)
				ctx.Dispose();

			_contexts = null;
		}

		public abstract Stages Variant { get; }

		public IReadOnlyStageContext<TRet>? TryGet(string guid)
		{
			return Contexts.TryGetValue(guid, out var context) ? context : null;
		}

		public IReadOnlyStageContext<TRet> this[string guid] => TryGet(guid) ?? throw new InvalidOperationException("The plugin" +
			$"with the GUID '{guid}' was not found.");

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

			var rctx = (IReadOnlyStageContext<TRet>) ctx;

			IReadOnlyStratumPlugin plugin = rctx.Plugin;

			// All this is just to validate that the attributes == loaders
			List<string> attributes = plugin.GetType()
				.GetCustomAttributes(typeof(StratumLoaderAttribute), true)
				.Cast<StratumLoaderAttribute>()
				.Where(v => v.Stage == Variant)
				.Select(v => v.Name)
				.ToList();
			List<string> loaders = rctx.Loaders.Select(v => v.Key).ToList();

			HashSet<string> shared = new(attributes);
			if (shared.Count < attributes.Count) // identical attributes
				throw new Exception("The plugin had one or more identical " + nameof(StratumLoaderAttribute) + "s.");

			shared.IntersectWith(loaders);

			foreach (var name in loaders)
				if (!shared.Contains(name))
					throw new Exception("The plugin did not have a " + nameof(StratumLoaderAttribute) + " for the loader named '" +
					                    name + "'");

			foreach (var name in attributes)
				if (!shared.Contains(name))
					throw new Exception("The plugin did not add the loader named '" + name + "', as declared by the " +
					                    nameof(StratumLoaderAttribute));

			Contexts.Add(plugin.Info.Metadata.GUID, ctx);
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
