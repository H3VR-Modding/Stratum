using Stratum.Internal.Staging.Events;

namespace Stratum
{
	/// <summary>
	///		Contains all the <see cref="IStageEvents"/> that can be subscribed to
	/// </summary>
	public sealed class AllStageEvents
	{
		/// <summary>
		///		Stage events for the setup stage
		/// </summary>
		public IStageEvents Setup { get; }

		/// <summary>
		///		Stage events for the runtime stage
		/// </summary>
		public IStageEvents Runtime { get; }

		/// <summary>
		///		Stage events for any of the stages
		/// </summary>
		public IStageEvents Any { get; }

		internal AllStageEvents(IStageEvents setup, IStageEvents runtime)
		{
			Setup = setup;
			Runtime = runtime;
			Any = new CollectiveStageEvents(Setup, Runtime);
		}
	}
}
