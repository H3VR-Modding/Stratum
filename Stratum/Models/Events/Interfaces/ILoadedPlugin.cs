namespace Stratum
{
	/// <summary>
	///		Represents a plugin that has been loaded
	/// </summary>
	public interface ILoadedPlugin : ITimed, IHasStage
	{
		/// <summary>
		///		The plugin that was loaded
		/// </summary>
		IReadOnlyStratumPlugin Plugin { get; }

		/// <summary>
		///		Whether or not the plugin was fully loaded (<see langword="true"/>) or errored (<see langword="false"/>)
		/// </summary>
		bool Success { get; }
	}
}
