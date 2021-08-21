using System;
using System.Collections;
using BepInEx.Logging;

namespace Stratum.Internal
{
	internal class ChainloaderCompleteHook : ILogListener
	{
		public static void Create(Action callback, CoroutineStarter startCoroutine)
		{
			ChainloaderCompleteHook listener = new(callback, startCoroutine);

			Logger.Listeners.Add(listener);
		}

		private readonly CoroutineStarter _startCoroutine;

		private Action? _callback;

		public ChainloaderCompleteHook(Action callback, CoroutineStarter startCoroutine)
		{
			_callback = callback;
			_startCoroutine = startCoroutine;
		}

		private IEnumerator DelayedRemove()
		{
			yield return null;

			Logger.Listeners.Remove(this);
		}

		public void LogEvent(object sender, LogEventArgs eventArgs)
		{
			if (_callback == null || eventArgs is not
				{ Source: { SourceName: "BepInEx" }, Data: "Chainloader startup complete" })
				return;

			_startCoroutine(DelayedRemove());
			_callback();

			_callback = null;
		}

		public void Dispose()
		{
			_callback = null;
		}
	}
}
