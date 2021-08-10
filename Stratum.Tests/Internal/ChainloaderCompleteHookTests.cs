using System;
using System.Collections;
using BepInEx.Logging;
using Moq;
using Stratum.Internal;
using Xunit;

namespace Stratum.Tests.Internal
{
	public class ChainloaderCompleteHookTests
	{
		private static LogEventArgs EventArgs(string source = "BepInEx", string data = "Chainloader startup complete")
		{
			return new(data, LogLevel.Info, new ManualLogSource(source));
		}

		[Fact]
		public void Log_IrrelevantLog_CallbackNotMade()
		{
			Mock<Action> callback = new();
			CoroutineStarter startCoroutine = Mock.Of<CoroutineStarter>();
			ChainloaderCompleteHook hook = new(callback.Object, startCoroutine);
			LogEventArgs argsNotBep = EventArgs(source: "Not BepInEx");
			LogEventArgs argsNotComplete = EventArgs(data: "Found 1 plugins to load");

			hook.LogEvent(this, argsNotBep);
			hook.LogEvent(this, argsNotComplete);

			callback.Verify(x => x(), Times.Never);
		}

		[Fact]
		public void Log_ChainloaderComplete_CallbackMade()
		{
			Mock<Action> callback = new();
			CoroutineStarter startCoroutine = Mock.Of<CoroutineStarter>();
			ChainloaderCompleteHook hook = new(callback.Object, startCoroutine);
			LogEventArgs args = EventArgs();

			hook.LogEvent(this, args);

			callback.Verify(x => x(), Times.Once);
		}

		[Fact]
		public void Log_MultipleChainloaderComplete_SingularCallbackMade()
		{
			Mock<Action> callback = new();
			CoroutineStarter startCoroutine = Mock.Of<CoroutineStarter>();
			ChainloaderCompleteHook hook = new(callback.Object, startCoroutine);
			LogEventArgs args = EventArgs();

			hook.LogEvent(this, args);
			hook.LogEvent(this, args);

			callback.Verify(x => x(), Times.Once);
		}

		[Fact]
		public void Log_ChainloaderComplete_RemovalMade()
		{
			Action callback = Mock.Of<Action>();
			Mock<CoroutineStarter> startCoroutine = new();
			ChainloaderCompleteHook hook = new(callback, startCoroutine.Object);
			LogEventArgs args = EventArgs();

			hook.LogEvent(this, args);

			startCoroutine.Verify(x => x(It.IsAny<IEnumerator>()), Times.Once);
		}

		[Fact]
		public void Log_Disposed_CallbackNotMade()
		{
			Mock<Action> callback = new();
			CoroutineStarter startCoroutine = Mock.Of<CoroutineStarter>();
			ChainloaderCompleteHook hook = new(callback.Object, startCoroutine);
			LogEventArgs args = EventArgs();

			hook.Dispose();
			hook.LogEvent(this, args);

			callback.Verify(x => x(), Times.Never);
		}
	}
}
