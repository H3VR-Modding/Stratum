using System;

namespace Stratum
{
	/// <summary>
	///     Represents the possible events of a stage's loading process
	/// </summary>
	public interface IStageEvents
	{
		/// <summary>
		///     Invoked when the stage begins to load
		/// </summary>
		event EventHandler<StageLoadingEventArgs>? StageLoading;

		/// <summary>
		///     Invoked when a batch begins to load
		/// </summary>
		event EventHandler<BatchLoadingEventArgs>? BatchLoading;

		/// <summary>
		///     Invoked when a plugin successfully finishes loading
		/// </summary>
		event EventHandler<PluginLoadedEventArgs>? PluginLoaded;

		/// <summary>
		///     Invoked when a plugin errors while loading
		/// </summary>
		event EventHandler<PluginFailedEventArgs>? PluginFailed;

		/// <summary>
		///     Invoked when a batch finishes loading
		/// </summary>
		event EventHandler<BatchLoadedEventArgs>? BatchLoaded;

		/// <summary>
		///     Invoked when a stage finishes loading
		/// </summary>
		event EventHandler<StageLoadedEventArgs>? StageLoaded;
	}
}
