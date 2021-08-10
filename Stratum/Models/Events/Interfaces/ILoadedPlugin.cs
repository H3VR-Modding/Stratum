namespace Stratum
{
	public interface ILoadedPlugin : ITimed, IHasStage
	{
		IReadOnlyStratumPlugin Plugin { get; }

		bool Success { get; }
	}
}
