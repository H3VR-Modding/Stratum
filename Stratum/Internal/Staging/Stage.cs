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
			if (_contexts == null)
				return;

			foreach (StageContext<TRet> ctx in _contexts.Values)
				ctx.Dispose();

			_contexts = null;
		}

		public abstract Stages Variant { get; }

		public IReadOnlyStageContext<TRet>? TryGet(string guid)
		{
			return Contexts.TryGetValue(guid, out StageContext<TRet> context) ? context : null;
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

			Plugin plugin = ctx.PluginInternal;

			// All this is just to validate that the attributes == loaders
			if (!plugin.Loaders.TryGetValue(Variant, out var attributes))
				attributes = new string[0];
			List<string> loaders = ctx.Loaders.Select(v => v.Key).ToList();

			HashSet<string> shared = new(attributes);
			if (shared.Count < attributes.Length) // duplicate attributes
				throw new Exception($"The plugin had one or more duplicate {Variant.ToFriendlyString()} loaders.");

			shared.IntersectWith(loaders);

			foreach (string name in loaders)
				if (!shared.Contains(name))
					throw new Exception($"The plugin did not declare the {Variant.ToFriendlyString()} loader named '{name}'");

			foreach (string name in attributes)
				if (!shared.Contains(name))
					throw new Exception($"The plugin did not add the declared {Variant.ToFriendlyString()} loader named '{name}'");

			Contexts.Add(plugin.Content.Info.Metadata.GUID, ctx);
		}

		protected abstract TRet BeginRun(StageContext<TRet> ctx);

		public TRet Run(Plugin plugin)
		{
			Logger.LogDebug($"Loading {plugin} -> {Variant}");

			return BeginRun(new StageContext<TRet>(this, plugin));
		}

		public override string ToString()
		{
			return Variant.ToString();
		}
	}
}
