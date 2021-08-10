using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using BepInEx;
using Stratum.Extensions;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Staging;
using Stratum.Internal.Staging.Events;

namespace Stratum.Internal
{
	internal readonly struct Bootstrap
	{
		private static Graph<IStratumPlugin, bool> PluginsToGraph(List<IStratumPlugin> plugins)
		{
			Dictionary<string, Graph<IStratumPlugin, bool>.Node> nodes = new(plugins.Count);
			Graph<IStratumPlugin, bool> graph = new(plugins);

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

		private readonly Scheduler _scheduler;
		private readonly EventInvocators _events;

		public Bootstrap(List<IStratumPlugin> plugins, EventInvocators events)
		{
			Graph<IStratumPlugin, bool> graph = PluginsToGraph(plugins);
			DependencyEnumerable<IStratumPlugin> deps = new(graph);
			_scheduler = new(deps);

			_events = events;
		}

		private void RunSetup()
        {
        	Scheduler.Runner<Empty> runner = Scheduler.ImmediateRunner;
            using Stage<Empty> stage = new(new SetupStageEssence(), _scheduler.Count);

            _scheduler.Run(stage, runner, _events.Setup);
        }

        private void RunRuntime(CoroutineStarter startCoroutine, Stopwatch clock)
        {
        	Scheduler.Runner<IEnumerator> runner = Scheduler.DelayedRunner(startCoroutine);

        	// Don't dispose this, it will die before the coroutine starts.
        	// Also, stuff in runtime can be used throughout runtime.
        	Stage<IEnumerator> stage = new(new RuntimeStageEssence(), _scheduler.Count);

            // Copy because closures cannot access fields, only locals
            EventInvocators events = _events;
            try
            {
	            // ReSharper disable once AccessToDisposedClosure	It's only disposed if the code throws, in which case the lambda wont run
	            IEnumerator exec = _scheduler
		            .Run(stage, runner, events.Runtime)
		            .TryFinally(() => stage.Dispose())
		            .ContinueWith(() =>
		            {
			            clock.Stop();

			            if (events.Complete is { } invoke)
			            {
				            LoadedStratumEventArgs loadedStratum = new(clock.Elapsed);

				            invoke(loadedStratum);
			            }
		            });

	            startCoroutine(exec);
            }
            catch // Don't use finally. This should only be ran when the enumerator fails to execute.
            {
	            stage.Dispose();

	            throw;
            }
        }

        public void Run(CoroutineStarter startCoroutine)
        {
	        Stopwatch clock = Stopwatch.StartNew();

	        RunSetup();
		    RunRuntime(startCoroutine, clock);
        }
	}
}
