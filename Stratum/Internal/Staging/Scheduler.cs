using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Stratum.Extensions;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Staging.Events;
using UnityEngine;

namespace Stratum.Internal.Staging
{
	internal class Scheduler
	{
		public delegate TRet Runner<TRet>(Stage<TRet> stage, DependencyEnumerable<IStratumPlugin> plugins,
			Invocators<TRet> events);

		public static Runner<Empty> ImmediateRunner { get; } = (stage, plugins, events) =>
		{
			Stopwatch stageClock = new();
			Stopwatch batchClock = new();
			Stopwatch pluginClock = new();

			ILoadedPlugin LoadPlugin(Graph<IStratumPlugin, bool>.Node node)
			{
				IStratumPlugin plugin = node.Metadata;

				pluginClock.Start();

				try
				{
					stage.Run(plugin);
				}
				catch (Exception e)
				{
					pluginClock.Stop();

					ILoadedPlugin failedPlugin = events.PluginFailed(node, pluginClock.Elapsed, e);

					pluginClock.Reset();

					return failedPlugin;
				}

				pluginClock.Stop();

				ILoadedPlugin loadedPlugin = events.PluginLoaded(pluginClock.Elapsed, plugin);

				pluginClock.Reset();

				return loadedPlugin;
			}

			ILoadedBatch LoadBatch(Graph<IStratumPlugin, bool>.Node[] batch, int index)
			{
				events.BatchLoading(batch, index);

				batchClock.Start();

				ILoadedPlugin[] loadedPlugins = Array.ConvertAll(batch, LoadPlugin);

				batchClock.Stop();

				ILoadedBatch loadedBatch = events.BatchLoaded(batchClock.Elapsed, index, loadedPlugins);

				batchClock.Reset();

				return loadedBatch;
			}

			void Load()
			{
				events.StageLoading();

				List<ILoadedBatch> loadedBatches = new();

				stageClock.Start();

				var i = 0;
				foreach (Graph<IStratumPlugin, bool>.Node[] batch in plugins)
					loadedBatches.Add(LoadBatch(batch, i++));

				stageClock.Stop();

				events.StageLoaded(stageClock.Elapsed, loadedBatches);
			}

			Load();

			return new Empty();
		};

		public static Runner<IEnumerator> DelayedRunner(CoroutineStarter startCoroutine)
		{
			return (stage, plugins, events) =>
			{
				IEnumerator Body()
				{
					Stopwatch stageClock = new();
					Stopwatch batchClock = new();

					events.StageLoading();

					stageClock.Start();

					Stack<Coroutine> concurrent = new();
					List<ILoadedBatch> loadedBatches = new();

					var i = 0;
					foreach (Graph<IStratumPlugin, bool>.Node[] batch in plugins)
					{
						events.BatchLoading(batch, i);

						batchClock.Start();

						var loadedPluginsBuffer = new ILoadedPlugin[batch.Length];

						// Start batch
						for (var j = 0; j < batch.Length; ++j)
						{
							Stopwatch pluginClock = Stopwatch.StartNew();
							Graph<IStratumPlugin, bool>.Node node = batch[j];
							IStratumPlugin plugin = node.Metadata;

							int k = j;
							IEnumerator pipeline = stage.Run(plugin)
								.TryCatch(e =>
								{
									pluginClock.Stop();

									loadedPluginsBuffer[k] = events.PluginFailed(node, pluginClock.Elapsed, e);
								}, () =>
								{
									pluginClock.Stop();

									loadedPluginsBuffer[k] = events.PluginLoaded(pluginClock.Elapsed, plugin);
								});
							Coroutine running = startCoroutine(pipeline);

							concurrent.Push(running);
						}

						// Await batch
						while (concurrent.Count > 0)
							yield return concurrent.Pop();

						batchClock.Stop();

						events.BatchLoaded(batchClock.Elapsed, i, loadedPluginsBuffer);

						batchClock.Reset();
					}

					stageClock.Stop();

					events.StageLoaded(stageClock.Elapsed, loadedBatches);

					++i;
				}

				return Body();
			};
		}

		private readonly DependencyEnumerable<IStratumPlugin> _plugins;

		public int Count => _plugins.Count;

		public Scheduler(DependencyEnumerable<IStratumPlugin> plugins)
		{
			_plugins = plugins;
		}

		public TRet Run<TRet>(Stage<TRet> stage, Runner<TRet> runner, DefaultStageEvents.Invocators events)
		{
			return runner(stage, _plugins, new Invocators<TRet>(this, stage, events));
		}

		public class Invocators<TRet>
		{
			private readonly Scheduler _scheduler;
			private readonly Stage<TRet> _stage;
			private readonly DefaultStageEvents.Invocators _events;

			public Invocators(Scheduler scheduler, Stage<TRet> stage, DefaultStageEvents.Invocators events)
			{
				_scheduler = scheduler;
				_stage = stage;
				_events = events;
			}

			public void StageLoading()
			{
				_events.StageLoading?.Invoke(new StageLoadingEventArgs(_stage.Variant));
			}

			public void BatchLoading(Graph<IStratumPlugin, bool>.Node[] batch, int index)
			{
				if (_events.BatchLoading is not { } invoke)
					return;

				IReadOnlyStratumPlugin[] batchPlugins =
					Array.ConvertAll(batch, x => (IReadOnlyStratumPlugin)x.Metadata);
				ReadOnlyList<IReadOnlyStratumPlugin> list = new(batchPlugins);
				BatchLoadingEventArgs args = new(_stage.Variant, index, list);

				invoke(args);
			}

			public ILoadedPlugin PluginLoaded(TimeSpan duration, IStratumPlugin plugin)
			{
				PluginLoadedEventArgs args = new(duration, _stage.Variant, plugin);

				_events.PluginLoaded?.Invoke(args);

				return args;
			}

			public ILoadedPlugin PluginFailed(Graph<IStratumPlugin, bool>.Node plugin, TimeSpan duration,
				Exception exception)
			{
				// Skip 1 because the first node killed is the node itself
				IEnumerable<IReadOnlyStratumPlugin> killed =
					_scheduler._plugins.Kill(plugin).Skip(1).Select(v => (IReadOnlyStratumPlugin)v.Metadata);
				ReadOnlyList<IReadOnlyStratumPlugin> list = new(killed.ToList());
				PluginFailedEventArgs args = new(duration, _stage.Variant, plugin.Metadata, list, exception);

				_events.PluginFailed?.Invoke(args);

				return args;
			}

			public ILoadedBatch BatchLoaded(TimeSpan duration, int index, ILoadedPlugin[] plugins)
			{
				ReadOnlyList<ILoadedPlugin> list = new(plugins);
				BatchLoadedEventArgs args = new(duration, _stage.Variant, index, list);

				_events.BatchLoaded?.Invoke(args);

				return args;
			}

			public void StageLoaded(TimeSpan duration, List<ILoadedBatch> batches)
			{
				if (_events.StageLoaded is not { } invoke)
					return;

				StageLoadedEventArgs args = new(duration, _stage.Variant,
					new ReadOnlyList<ILoadedBatch>(batches));

				invoke(args);
			}
		}
	}
}
