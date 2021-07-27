using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using Stratum.Extensions;
using Stratum.Internal.Dependencies;
using Stratum.Internal.Staging;
using UnityEngine;

namespace Stratum.Internal.Scheduling
{
	internal sealed class DelayedScheduler : Scheduler<IEnumerator>
	{
		private readonly CoroutineStarter _startCoroutine;

		public DelayedScheduler(ManualLogSource logger, DependencyEnumerable<Plugin> plugins, CoroutineStarter startCoroutine)
			: base(logger, plugins)
		{
			_startCoroutine = startCoroutine;
		}

		public override IEnumerator Run(Stage<IEnumerator> stage)
		{
			Stack<Coroutine> concurrent = new();

			foreach (IEnumerable<Graph<Plugin, bool>.Node> batch in Plugins)
			{
				// Start batch
				foreach (Graph<Plugin, bool>.Node plugin in batch)
				{
					IEnumerator pipeline = stage.Run(plugin.Metadata).TryCatch(e => ContextException(plugin, stage, e));
					Coroutine running = _startCoroutine(pipeline);

					concurrent.Push(running);
				}

				// Await batch
				while (concurrent.Count > 0)
					yield return concurrent.Pop();
			}
		}
	}
}
