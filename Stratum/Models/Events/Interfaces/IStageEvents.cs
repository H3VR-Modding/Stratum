using System;

namespace Stratum
{
	public interface IStageEvents
	{
		event EventHandler<StageLoadingEventArgs>? StageLoading;
		event EventHandler<StageLoadedEventArgs>? StageLoaded;

		event EventHandler<BatchLoadingEventArgs>? BatchLoading;
		event EventHandler<BatchLoadedEventArgs>? BatchLoaded;

		event EventHandler<PluginLoadedEventArgs>? PluginLoaded;
		event EventHandler<PluginFailedEventArgs>? PluginFailed;
	}
}
