using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using Stratum.Internal;
using Stratum.Internal.Extensions;
using Stratum.Internal.Staging.Events;

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

		private static readonly DefaultStageEvents SetupEvents = new();
		private static readonly DefaultStageEvents RuntimeEvents = new();

		private static StratumRoot? _instance;

		public static AllStageEvents StageEvents { get; } = new(SetupEvents, RuntimeEvents);

		public static event EventHandler<LoadedStratumEventArgs>? LoadingComplete;

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

		private void Awake()
		{
			_instance = this;

			ChainloaderCompleteHook.Create(Load, StartCoroutine);
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

		private void Load()
		{
			_started = true;

			SubscribeLogs();

			Action<LoadedStratumEventArgs>? CompleteEventGetter()
			{
				return LoadingComplete is { } invoke
					? args => invoke(null, args)
					: null;
			}

			EventInvocators invocators = new(CompleteEventGetter, SetupEvents.Invoke, RuntimeEvents.Invoke);

			new Bootstrap(_plugins, invocators).Run(StartCoroutine);
		}

		private void SubscribeLogs()
		{
			void StageLoading(object sender, StageLoadingEventArgs args)
			{
				Logger.LogDebug($"Loading {args.Stage.ToFriendlyString()} stage");
			}

			void BatchLoading(object sender, BatchLoadingEventArgs args)
			{
				StringBuilder builder = new StringBuilder("Loading  ")
					.AppendExt(args)
					.Append(':').AppendLine();

				IReadOnlyList<IReadOnlyStratumPlugin> plugins = args.Plugins;
				for (var i = 0; i < plugins.Count; ++i)
					builder.Append(i + 1).Append(": ").AppendExt(plugins[i]).AppendLine();

				Logger.LogDebug(builder.ToString());
			}

			void PluginLoaded(object sender, PluginLoadedEventArgs args)
			{
				StringBuilder builder = new StringBuilder("Loaded ")
					.AppendExt(args.Plugin)
					.Append(" into the ")
					.AppendExt(args.Stage)
					.Append(" stage in ")
					.AppendExt(args);

				Logger.LogDebug(builder.ToString());
			}

			void PluginFailed(object sender, PluginFailedEventArgs args)
			{
				StringBuilder builder = new();

				builder
					.Append("An exception was thrown by ")
					.AppendExt(args.Plugin)
					.Append(" during the ")
					.AppendExt(args.Stage)
					.Append(" stage");

				{
					using IEnumerator<IReadOnlyStratumPlugin> enumerator = args.Dependents.GetEnumerator();

					bool Next()
					{
						return enumerator.MoveNext();
					}

					if (Next())
					{
						builder.Append(". The following hard-dependencies will not be loaded: ");

						var i = 0;
						do
						{
							builder.AppendLine();

							IReadOnlyStratumPlugin dependent = enumerator.Current!;

							builder.Append(++i).Append(": ").Append(dependent.Info);
						} while (Next());
					}
				}

				Logger.LogError(builder.ToString());
			}

			void BatchLoaded(object sender, BatchLoadedEventArgs args)
			{
				StringBuilder builder = new StringBuilder("Loaded ")
					.AppendExt((IEventBatch<ILoadedPlugin>) args)
					.Append(':')
					.AppendLine();

				IReadOnlyList<ILoadedPlugin> plugins = args.Plugins;
				for (var i = 0; i < plugins.Count; ++i)
				{
					ILoadedPlugin plugin = plugins[i];

					builder
						.Append(i + 1)
						.Append(": ")
						.AppendExt(plugin.Plugin)
						.Append(' ')
						.Append(plugin.Success ? "succeeded" : "failed")
						.Append(" in ")
						.AppendExt(plugin)
						.AppendLine();
				}

				Logger.LogDebug(builder.ToString());
			}

			void StageLoaded(object sender, StageLoadedEventArgs args)
			{
				StringBuilder builder = new StringBuilder("Loaded ")
					.Append(args.Stage.ToFriendlyString())
					.Append(" stage in ")
					.AppendExt(args)
					.Append(':')
					.AppendLine();

				foreach (ILoadedBatch batch in args.Batches)
				{
					builder
						.Append(batch.Index + 1)
						.Append(": ")
						.Append(batch.Plugins.Count)
						.Append(" plugins in ")
						.AppendExt((IEventBatch<ILoadedPlugin>) batch)
						.AppendLine();
				}

				Logger.LogDebug(builder.ToString());
			}

			void LoadingComplete(object sender, LoadedStratumEventArgs args)
			{
				Logger.LogMessage($"Loading complete ({args.Duration.TotalSeconds:F3}s)");
			}

			StageEvents.Any.StageLoading += StageLoading;
			StageEvents.Any.BatchLoading += BatchLoading;
			StageEvents.Any.PluginLoaded += PluginLoaded;
			StageEvents.Any.PluginFailed += PluginFailed;
			StageEvents.Any.BatchLoaded += BatchLoaded;
			StageEvents.Any.StageLoaded += StageLoaded;
			StratumRoot.LoadingComplete += LoadingComplete;
		}
	}
}
