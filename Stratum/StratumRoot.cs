using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Stratum.Extensions;
using Stratum.Internal;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Scheduling;
using Stratum.Internal.Staging;

namespace Stratum
{
	/// <summary>
	///		The Stratum BepInEx plugin
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
		private const string ExactVersion = MajorMinorVersion + "0";

		/// <summary>
		///		The BepInEx GUID used by Stratum
		/// </summary>
		public const string GUID = "stratum";
		/// <summary>
		///		The version of Stratum with a zeroed patch component. To find the exact version, use the BepInEx API
		/// </summary>
		public const string Version = MajorMinorVersion + "0";

		private static StratumRoot? _instance;

		private readonly List<Plugin> _plugins = new();
		private bool _started;

		private void Awake()
		{
			_instance = this;

			ChainloaderLogListener listener = new();

			void Callback()
			{
				listener.Callback -= Callback;
				Load();
			}

			listener.Callback += Callback;

			BepInEx.Logging.Logger.Listeners.Add(listener);
		}

		/// <summary>
		///		Adds a plugin to Stratum, allowing it to utilize all the benefits of the Stratum ecosystem
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

		private Graph<Plugin, bool> PluginsToGraph()
		{
			Dictionary<string, Graph<Plugin, bool>.Node> nodes = new(_plugins.Count);
			Graph<Plugin, bool> graph = new(_plugins);

			foreach (Graph<Plugin, bool>.Node node in graph)
			{
				IStratumPlugin plugin = node.Metadata.Content;
				PluginInfo info = plugin.Info;

				foreach (BepInDependency reference in info.Dependencies)
				{
					// This is unintuitive, but let me explain. This means either/both:
					// 1. The dependency is soft, because BepInEx didn't load it and BepInEx would load a hard-dependent.
					// 2. The plugin isn't a Stratum plugin, because it never injected.
					// In either situation, we don't care.
					if (!nodes.TryGetValue(reference.DependencyGUID, out Graph<Plugin, bool>.Node? resolved))
						continue;

					bool isHard = reference.Flags.HasFlag(BepInDependency.DependencyFlags.HardDependency);
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

			Graph<Plugin, bool> graph = PluginsToGraph();
			DependencyEnumerable<Plugin> deps = new(graph);

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
			if (_plugins.Any(x => x.Content == plugin))
			{
				Logger.LogWarning($"{plugin.Info} attempted to inject itself again.");
				return;
			}

			_plugins.Add(new Plugin(plugin));
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
