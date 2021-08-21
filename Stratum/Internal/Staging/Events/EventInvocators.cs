using System;

namespace Stratum.Internal.Staging.Events
{
	internal class EventInvocators
	{
		private readonly Func<Action<LoadedStratumEventArgs>?> _complete;

		public EventInvocators(Func<Action<LoadedStratumEventArgs>?> complete, DefaultStageEvents.Invocators setup,
			DefaultStageEvents.Invocators runtime)
		{
			_complete = complete;
			Setup = setup;
			Runtime = runtime;
		}

		public Action<LoadedStratumEventArgs>? Complete => _complete();
		public DefaultStageEvents.Invocators Setup { get; }
		public DefaultStageEvents.Invocators Runtime { get; }
	}
}
