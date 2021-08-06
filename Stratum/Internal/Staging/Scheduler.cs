using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Stratum.Extensions;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Staging;
using UnityEngine;

namespace Stratum.Internal.Staging
{
	internal class Scheduler
	{
		internal delegate TRet Runner<TRet>(Stage<TRet> stage, DependencyEnumerable<IStratumPlugin> plugins, Catch @catch);
		internal delegate void Catch(Graph<IStratumPlugin, bool>.Node plugin, Exception exception);

		public static Runner<Empty> ImmediateRunner { get; } = (stage, plugins, @catch) =>
		{
			foreach (Graph<IStratumPlugin, bool>.Node plugin in plugins.SelectMany(v => v))
				try
				{
					stage.Run(plugin.Metadata);
				}
				catch (Exception e)
				{
					@catch(plugin, e);
				}

			return new Empty();
		};

		public static Runner<IEnumerator> DelayedRunner(CoroutineStarter startCoroutine)
		{
			return (stage, plugins, @catch) =>
			{
				IEnumerator Body()
				{
					Stack<Coroutine> concurrent = new();

					foreach (IEnumerable<Graph<IStratumPlugin, bool>.Node> batch in plugins)
					{
						// Start batch
						foreach (Graph<IStratumPlugin, bool>.Node plugin in batch)
						{
							IEnumerator pipeline = stage.Run(plugin.Metadata).TryCatch(e => @catch(plugin, e));
							Coroutine running = startCoroutine(pipeline);

							concurrent.Push(running);
						}

						// Await batch
						while (concurrent.Count > 0)
							yield return concurrent.Pop();
					}
				}

				return Body();
			};
		}

		private readonly ManualLogSource _logger;
		private readonly DependencyEnumerable<IStratumPlugin> _plugins;

		public int Count => _plugins.Count;

		public Scheduler(ManualLogSource logger, DependencyEnumerable<IStratumPlugin> plugins)
		{
			_logger = logger;
			_plugins = plugins;
		}

		private Catch ContextException<TRet>(Stage<TRet> stage)
		{
			return (plugin, exception) =>
			{
				string[] killed = _plugins.Kill(plugin).Select(v => v.Metadata.Info.ToString()).Skip(1).ToArray();
				string cascade = string.Join(", ", killed);

				var message = $"{plugin.Metadata.Info} caused an exception during {stage}.";
				if (killed.Length > 0)
					message += $"The following hard-dependents will not be loaded: {cascade}";

				message += "\n" + exception;

				_logger.LogError(message);
			};
		}

		public TRet Run<TRet>(Stage<TRet> stage, Runner<TRet> runner)
		{
			return runner(stage, _plugins, ContextException(stage));
		}
	}
}
