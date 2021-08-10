using System;

namespace Stratum.Internal.Staging.Events
{
	internal class EventInvocators
	{
		public EventInvocators(Func<Action<LoadedStratumEventArgs>?> complete, DefaultStageEvents.Invocators setup, DefaultStageEvents.Invocators runtime)
		{
			_complete = complete;
			Setup = setup;
			Runtime = runtime;
		}

		private readonly Func<Action<LoadedStratumEventArgs>?> _complete;

		public Action<LoadedStratumEventArgs>? Complete => _complete();
		public DefaultStageEvents.Invocators Setup { get; }
		public DefaultStageEvents.Invocators Runtime { get; }
	}
}
