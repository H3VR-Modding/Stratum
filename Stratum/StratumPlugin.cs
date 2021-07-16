using System.Collections;
using System.IO;
using BepInEx;

namespace Stratum
{
	public abstract class StratumPlugin : BaseUnityPlugin, IStratumPlugin
	{
		protected PluginDirectories Directories { get; }

		protected StratumPlugin()
		{
			Directories = new PluginDirectories(new(Path.GetDirectoryName(Info.Location)!));

			StratumRoot.Inject(this);
		}

		public abstract void OnSetup(IStageContext<Empty> ctx);

		public abstract IEnumerator OnRuntime(IStageContext<IEnumerator> ctx);
	}
}
