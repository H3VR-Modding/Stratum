using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Stratum.Extensions;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Scheduling;
using Stratum.Internal.Staging;

namespace Stratum
{
	[BepInPlugin(GUID, "Stratum", Version)]
	public sealed class StratumRoot : BaseUnityPlugin
	{
		public const string GUID = "stratum";
		public const string Version = "1.0.0";

		private static StratumRoot? _instance;

		public static void Inject(IStratumPlugin plugin)
		{
			if (_instance == null)
				throw new InvalidOperationException(
					"Stratum has not yet initialised! Please ensure your BepInEx plugin depends on 'stratum'.");

			_instance.InjectInstance(plugin);
		}

		private readonly List<IStratumPlugin> _plugins = new();
		private bool _started;

		private void Awake()
		{
			_instance = this;

			var listener = new ChainloaderLogListener();

			void Callback()
			{
				listener.Callback -= Callback;
				Load();
			}

			listener.Callback += Callback;

			BepInEx.Logging.Logger.Listeners.Add(listener);
		}

		private Graph<IStratumPlugin, bool> ModsToGraph()
		{
			var nodes = new Dictionary<string, Graph<IStratumPlugin, bool>.Node>(_plugins.Count);
			var graph = new Graph<IStratumPlugin, bool>(_plugins);

			foreach (var node in graph)
			{
				var plugin = node.Metadata;
				var info = plugin.Info;

				foreach (var reference in info.Dependencies)
				{
					// This is unintuitive, but let me explain. This means either/both:
					// 1. The dependency is soft, because BepInEx didn't load it and BepInEx wouldn't load a hard-dependent.
					// 2. The plugin isn't a Stratum plugin, because it never injected.
					// In either situation, we don't care.
					if (!nodes.TryGetValue(reference.DependencyGUID, out var resolved))
						continue;

					var isHard = reference.Flags.HasFlag(BepInDependency.DependencyFlags.HardDependency);
					node.Attach(resolved, isHard);
				}

				nodes.Add(info.Metadata.GUID, node);
			}

			return graph;
		}

		private void Load()
		{
			_started = true;

			// ggez
			if (_plugins.Count == 0)
				return;

			var graph = ModsToGraph();
			var deps = new DependencyEnumerable<IStratumPlugin>(graph);

			{
				var immediate = new ImmediateScheduler(Logger, deps);
				using var stage = new SetupStage(_plugins.Count, Logger);

				try
				{
					immediate.Run(stage);
				}
				catch (Exception e)
				{
					Logger.LogFatal("An unhandled exception was thrown by the immediate stage scheduler. A stage may have been " +
					                "interrupted, and no further stages will be loaded:\n" + e);
					return;
				}
			}

			{
				var delayed = new DelayedScheduler(Logger, deps, StartCoroutine);
				// Don't dispose this, it will die before the coroutine starts.
				// Also, stuff in runtime can be used throughout runtime.
				var stage = new RuntimeStage(_plugins.Count, Logger);

				IEnumerator exec;
				try
				{
					exec = delayed
						.Run(stage)
						.TryCatch(e => Logger.LogFatal("An unhandled exception was thrown by the delayed stage scheduler " +
						                               "(mid-yield). A stage may have been interrupted, and no further stages will be " +
						                               "loaded:\n" + e));
				}
				catch (Exception e)
				{
					Logger.LogFatal("An unhandled exception was thrown by the delayed stage scheduler (pre-yield). No further stages " +
					                "will be loaded:\n" + e);
					return;
				}

				StartCoroutine(exec);
			}
		}

		private void InjectInstance(IStratumPlugin plugin)
		{
			// BepInEx.ScriptEngine
			if (_started)
				throw new InvalidOperationException("Plugins cannot be injected after stage execution begins. This is likely " +
				                                    "from the plugin being reloaded via BepInEx.ScriptEngine.");

			// Avoid duplicating any plugins
			if (_plugins.Contains(plugin))
			{
				Logger.LogWarning($"{plugin} attempted to inject itself again.");
				return;
			}

			_plugins.Add(plugin);
		}

		private class ChainloaderLogListener : ILogListener
		{
			public event Action? Callback;

			public void LogEvent(object sender, LogEventArgs eventArgs)
			{
				if (eventArgs.Source.SourceName != "BepInEx" || eventArgs.Data.ToString() != "Chainloader startup complete")
					return;

				// VERY IMPORTANT
				// Remove the listener BEFORE invoking callback
				// The callback might use a logger, which would call this again, causing a recursive loop
				BepInEx.Logging.Logger.Listeners.Remove(this);
				Callback?.Invoke();
			}

			public void Dispose() { }
		}
	}
}
