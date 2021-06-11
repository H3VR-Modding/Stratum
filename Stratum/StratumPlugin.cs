using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Stratum.IO;

namespace Stratum
{
	public abstract class StratumPlugin : BaseUnityPlugin, IStratumPlugin
	{
		protected PluginDirectories Directories { get; }

		IEnumerable<StratumLoaderAttribute> IReadOnlyStratumPlugin.Loaders =>
			GetType().GetCustomAttributes(typeof(StratumLoaderAttribute), true).Cast<StratumLoaderAttribute>();

		protected StratumPlugin()
		{
			Directories = new PluginDirectories(new(Path.GetDirectoryName(Info.Location)!));

			StratumRoot.Inject(this);
		}

		public abstract void OnSetup(IStageContext<Empty> ctx);

		public abstract IEnumerator OnRuntime(IStageContext<IEnumerator> ctx);
	}
}
