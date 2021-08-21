using System;

namespace Stratum.Internal.Staging.Events
{
	internal class CollectiveStageEvents : IStageEvents
	{
		public CollectiveStageEvents(params IStageEvents[] events)
		{
			_events = events;
		}

		private readonly IStageEvents[] _events;

		public event EventHandler<StageLoadingEventArgs>? StageLoading
		{
			add => Foreach(value, (state, item) => item.StageLoading += state);
			remove => Foreach(value, (state, item) => item.StageLoading -= state);
		}

		public event EventHandler<BatchLoadingEventArgs>? BatchLoading
		{
			add => Foreach(value, (state, item) => item.BatchLoading += state);
			remove => Foreach(value, (state, item) => item.BatchLoading -= state);
		}

		public event EventHandler<PluginLoadedEventArgs>? PluginLoaded
		{
			add => Foreach(value, (state, item) => item.PluginLoaded += state);
			remove => Foreach(value, (state, item) => item.PluginLoaded -= state);
		}

		public event EventHandler<PluginFailedEventArgs>? PluginFailed
		{
			add => Foreach(value, (state, item) => item.PluginFailed += state);
			remove => Foreach(value, (state, item) => item.PluginFailed -= state);
		}

		public event EventHandler<BatchLoadedEventArgs>? BatchLoaded
		{
			add => Foreach(value, (state, item) => item.BatchLoaded += state);
			remove => Foreach(value, (state, item) => item.BatchLoaded -= state);
		}

		public event EventHandler<StageLoadedEventArgs>? StageLoaded
		{
			add => Foreach(value, (state, item) => item.StageLoaded += state);
			remove => Foreach(value, (state, item) => item.StageLoaded -= state);
		}

		private void Foreach<T>(T value, Action<T, IStageEvents> action)
		{
			foreach (IStageEvents item in _events)
				action(value, item);
		}
	}
}
