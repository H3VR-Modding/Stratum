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
	// if you get a warning that there is no attribute, just build the project
	public sealed partial class StratumRoot : BaseUnityPlugin
	{
		private const string Name = "Stratum";

		/// <summary>
		///     The BepInEx GUID used by Stratum
		/// </summary>
		public const string GUID = "stratum";

		private static readonly DefaultStageEvents SetupEvents = new();
		private static readonly DefaultStageEvents RuntimeEvents = new();

		private static StratumRoot? _instance;

		private readonly List<IStratumPlugin> _plugins = new();

		private bool _started;

		/// <summary>
		///     All the <see cref="IStageEvents" /> that can be subscribed to
		/// </summary>
		public static AllStageEvents StageEvents { get; } = new(SetupEvents, RuntimeEvents);

		/// <summary>
		///     Invoked when Stratum finishes loading all stages
		/// </summary>
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

		private void Awake()
		{
			_instance = this;

			ChainloaderCompleteHook.Create(Load, StartCoroutine);
		}

		private void InjectInstance(IStratumPlugin plugin)
		{
			// BepInEx.ScriptEngine
			if (_started)
				throw new InvalidOperationException(
					"Plugins cannot be injected after stage execution begins. This is likely " +
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
				StringBuilder builder = new StringBuilder("Loading ")
					.AppendExt(args.Stage)
					.Append(" stage");

				Logger.LogDebug(builder.ToString());
			}

			void BatchLoading(object sender, BatchLoadingEventArgs args)
			{
				StringBuilder builder = new StringBuilder("Loading ")
					.AppendExt(args)
					.Append(':');

				IReadOnlyList<IReadOnlyStratumPlugin> plugins = args.Plugins;
				for (var i = 0; i < plugins.Count; ++i)
					builder.AppendLine().Append(i + 1).Append(": ").AppendExt(plugins[i]);

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

					bool Next() => enumerator.MoveNext();

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
					.AppendExt((IEventBatch<ILoadedPlugin>)args)
					.Append(':');

				IReadOnlyList<ILoadedPlugin> plugins = args.Plugins;
				for (var i = 0; i < plugins.Count; ++i)
				{
					ILoadedPlugin plugin = plugins[i];

					builder
						.AppendLine()
						.Append(i + 1)
						.Append(": ")
						.AppendExt(plugin.Plugin)
						.Append(' ')
						.Append(plugin.Success ? "succeeded" : "failed")
						.Append(" in ")
						.AppendExt(plugin);
				}

				Logger.LogDebug(builder.ToString());
			}

			void StageLoaded(object sender, StageLoadedEventArgs args)
			{
				StringBuilder builder = new StringBuilder("Loaded ")
					.Append(args.Stage.ToFriendlyString())
					.Append(" stage in ")
					.AppendExt(args)
					.Append(':');

				foreach (ILoadedBatch batch in args.Batches)
					builder
						.AppendLine()
						.Append(batch.Generation + 1)
						.Append(": ")
						.Append(batch.Plugins.Count)
						.Append(" plugins in ")
						.AppendExt((IEventBatch<ILoadedPlugin>)batch);

				Logger.LogDebug(builder.ToString());
			}

			void LoadingComplete(object sender, LoadedStratumEventArgs args)
			{
				StringBuilder builder = new StringBuilder("Loading complete (")
					.AppendExt(args)
					.Append(')');

				Logger.LogMessage(builder.ToString());
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
