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
	/// <summary>
	///     The Stratum BepInEx plugin
	/// </summary>
	[BepInPlugin(GUID, "Stratum", ExactVersion)]
	public sealed class StratumRoot : BaseUnityPlugin
	{
		private const string MajorMinorVersion = "1.0.";

		// This is 'Version' but with the patch component not zeroed
		// People can use 'Version' in their 'BepInDependency's, but not 'ExactVersion', because there is no good reason to depend on the
		// patch component.
		// 'Version' should be intentionally rough to prevent people from baking in the full Stratum version in places other than the
		// dependency attribute too. If a plugin is built on 1.0.0, and 1.0.1 releases, the plugin would still think its 1.0.0 because
		// 'Version' is a constant.
		private const string ExactVersion = MajorMinorVersion + "1";

		/// <summary>
		///     The BepInEx GUID used by Stratum
		/// </summary>
		public const string GUID = "stratum";

		/// <summary>
		///     The version of Stratum with a zeroed patch component. To find the exact version, use the BepInEx API
		/// </summary>
		public const string Version = MajorMinorVersion + "0";

		private static StratumRoot? _instance;

		/// <summary>
		///     Adds a plugin to Stratum, allowing it to utilize all the benefits of the Stratum ecosystem
		/// </summary>
		/// <param name="plugin">The plugin to add to Stratum</param>
		/// <exception cref="InvalidOperationException">Stratum has not yet initialised or it has already started</exception>
		public static void Inject(IStratumPlugin plugin)
		{
			if (_instance == null)
				throw new InvalidOperationException(
					"Stratum has not yet initialised! Please ensure your BepInEx plugin depends on 'stratum'.");

			_instance.InjectInstance(plugin);
		}

		private readonly List<IStratumPlugin> _plugins = new();

		private bool _started;
		private ChainloaderLogListener? _listener;

		private void Awake()
		{
			_instance = this;

			_listener = new ChainloaderLogListener(Load);

			BepInEx.Logging.Logger.Listeners.Add(_listener);
		}

		private Graph<IStratumPlugin, bool> PluginsToGraph()
		{
			Dictionary<string, Graph<IStratumPlugin, bool>.Node> nodes = new(_plugins.Count);
			Graph<IStratumPlugin, bool> graph = new(_plugins);

			foreach (Graph<IStratumPlugin, bool>.Node node in graph)
			{
				IStratumPlugin plugin = node.Metadata;
				PluginInfo info = plugin.Info;

				foreach (BepInDependency reference in info.Dependencies)
				{
					// This is unintuitive, but let me explain. This means either/both:
					// 1. The dependency is soft, because BepInEx didn't load it and BepInEx would load a hard-dependent.
					// 2. The plugin isn't a Stratum plugin, because it never injected.
					// In either situation, we don't care.
					if (!nodes.TryGetValue(reference.DependencyGUID, out Graph<IStratumPlugin, bool>.Node? resolved))
						continue;

					bool isHard = reference.Flags.HasFlagFast(BepInDependency.DependencyFlags.HardDependency);
					node.Attach(resolved, isHard);
				}

				nodes.Add(info.Metadata.GUID, node);
			}

			return graph;
		}

		private IEnumerator UnsubscribeLog()
		{
			yield return null;

			BepInEx.Logging.Logger.Listeners.Remove(_listener);
		}

		private void Load()
		{
			_started = true;
			StartCoroutine(UnsubscribeLog());

			// ggez
			if (_plugins.Count == 0)
				return;

			Graph<IStratumPlugin, bool> graph = PluginsToGraph();
			DependencyEnumerable<IStratumPlugin> deps = new(graph);

			{
				ImmediateScheduler scheduler = new(Logger, deps);
				using SetupStage stage = new(_plugins.Count, Logger);

				try
				{
					scheduler.Run(stage);
				}
				catch (Exception e)
				{
					Logger.LogFatal("An unhandled exception was thrown by the immediate stage scheduler. A stage may have been " +
					                "interrupted, and no further stages will be loaded:\n" + e);
					return;
				}
			}

			{
				DelayedScheduler scheduler = new(Logger, deps, StartCoroutine);
				// Don't dispose this, it will die before the coroutine starts.
				// Also, stuff in runtime can be used throughout runtime.
				RuntimeStage stage = new(_plugins.Count, Logger);

				IEnumerator exec;
				try
				{
					exec = scheduler
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

				StartCoroutine(exec.ContinueWith(() => Logger.LogMessage("Loading complete")));
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
				Logger.LogWarning($"{plugin.Info} attempted to inject itself again.");
				return;
			}

			_plugins.Add(plugin);
		}

		private class ChainloaderLogListener : ILogListener
		{
			private readonly Action _callback;

			public ChainloaderLogListener(Action callback)
			{
				_callback = callback;
			}

			public void LogEvent(object sender, LogEventArgs eventArgs)
			{
				if (eventArgs is {Source: {SourceName: "BepInEx"}, Data: "Chainloader startup complete"})
					_callback();
			}

			public void Dispose() { }
		}
	}
}
