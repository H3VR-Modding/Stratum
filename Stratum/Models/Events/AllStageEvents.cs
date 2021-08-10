using Stratum.Internal.Staging.Events;

namespace Stratum
{
	public class AllStageEvents
	{
		public IStageEvents Setup { get; }

		public IStageEvents Runtime { get; }

		public IStageEvents Any { get; }

		internal AllStageEvents(IStageEvents setup, IStageEvents runtime)
		{
			Setup = setup;
			Runtime = runtime;
			Any = new CollectiveStageEvents(Setup, Runtime);
		}
	}
}
