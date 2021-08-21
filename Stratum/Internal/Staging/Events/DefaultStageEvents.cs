using System;

namespace Stratum.Internal.Staging.Events
{
	internal class DefaultStageEvents : IStageEvents
	{
		internal DefaultStageEvents()
		{
			Invoke = new Invocators(this);
		}

		public Invocators Invoke { get; }

		public event EventHandler<StageLoadingEventArgs>? StageLoading;
		public event EventHandler<BatchLoadingEventArgs>? BatchLoading;
		public event EventHandler<PluginLoadedEventArgs>? PluginLoaded;
		public event EventHandler<PluginFailedEventArgs>? PluginFailed;
		public event EventHandler<BatchLoadedEventArgs>? BatchLoaded;
		public event EventHandler<StageLoadedEventArgs>? StageLoaded;

		public readonly struct Invocators
		{
			public Invocators(DefaultStageEvents events)
			{
				_events = events;
			}

			private readonly DefaultStageEvents _events;

			public Action<StageLoadingEventArgs>? StageLoading => Invocator(events => events.StageLoading);
			public Action<BatchLoadingEventArgs>? BatchLoading => Invocator(events => events.BatchLoading);
			public Action<PluginLoadedEventArgs>? PluginLoaded => Invocator(events => events.PluginLoaded);
			public Action<PluginFailedEventArgs>? PluginFailed => Invocator(events => events.PluginFailed);
			public Action<BatchLoadedEventArgs>? BatchLoaded => Invocator(events => events.BatchLoaded);
			public Action<StageLoadedEventArgs>? StageLoaded => Invocator(events => events.StageLoaded);

			private Action<T>? Invocator<T>(Func<DefaultStageEvents, EventHandler<T>?> @event) where T : EventArgs
			{
				// Copy because closures cannot use fields, only locals
				DefaultStageEvents events = _events;

				return @event(events) is { } invoke
					? args => invoke(events, args)
					: null;
			}
		}
	}
}
