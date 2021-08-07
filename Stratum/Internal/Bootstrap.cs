using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Stratum.Extensions;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Staging;

namespace Stratum.Internal
{
	internal static class Bootstrap
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

		private static bool RunSetup(ManualLogSource logger, Scheduler scheduler)
        {
        	Scheduler.Runner<Empty> runner = Scheduler.ImmediateRunner;

        	using Stage<Empty> stage = new(new SetupStageEssence(), scheduler.Count, logger);

        	try
        	{
        		scheduler.Run(stage, runner);
        	}
        	catch (Exception e)
        	{
	            logger.LogFatal("An unhandled exception was thrown by the immediate stage scheduler. A stage may have been " +
	                            "interrupted, and no further stages will be loaded:\n" + e);

        		return false;
        	}

        	return true;
        }

        private static void RunRuntime(ManualLogSource logger, Scheduler scheduler, CoroutineStarter startCoroutine)
        {
        	Scheduler.Runner<IEnumerator> runner = Scheduler.DelayedRunner(startCoroutine);

        	// Don't dispose this, it will die before the coroutine starts.
        	// Also, stuff in runtime can be used throughout runtime.
        	Stage<IEnumerator> stage = new(new RuntimeStageEssence(), scheduler.Count, logger);

        	IEnumerator exec;
        	try
        	{
        		exec = scheduler
        			.Run(stage, runner)
        			.TryCatch(e => logger.LogFatal("An unhandled exception was thrown by the delayed stage scheduler " +
                                                   "(mid-yield). A stage may have been interrupted, and no further stages will be " +
                                                   "loaded:\n" + e));
        	}
        	catch (Exception e)
        	{
        		logger.LogFatal("An unhandled exception was thrown by the delayed stage scheduler (pre-yield). No further stages " +
        		                "will be loaded:\n" + e);
        		return;
        	}

            startCoroutine(exec.ContinueWith(() => logger.LogMessage("Loading complete")));
        }

        public static void Run(ManualLogSource logger, List<IStratumPlugin> plugins, CoroutineStarter startCoroutine)
        {
	        Graph<IStratumPlugin, bool> graph = PluginsToGraph(plugins);
	        DependencyEnumerable<IStratumPlugin> deps = new(graph);
	        Scheduler scheduler = new(logger, deps);

	        if (RunSetup(logger, scheduler))
		        RunRuntime(logger, scheduler, startCoroutine);
        }
	}
}
