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

		public DelayedScheduler(ManualLogSource logger, DependencyEnumerable<IStratumPlugin> plugins, CoroutineStarter startCoroutine)
			: base(logger, plugins)
		{
			_startCoroutine = startCoroutine;
		}

		public override IEnumerator Run(Stage<IEnumerator> stage)
		{
			var concurrent = new Stack<Coroutine>();

			foreach (var batch in Plugins)
			{
				// Start batch
				foreach (var plugin in batch)
				{
					var pipeline = stage.Run(plugin.Metadata).TryCatch(e => ContextException(plugin, stage, e));
					var coroutine = _startCoroutine(pipeline);

					concurrent.Push(coroutine);
				}

				// Await batch
				while (concurrent.Count > 0)
				{
					yield return concurrent.Pop();
				}
			}
		}
	}
}
