using System.Collections;
using System.IO;
using BepInEx;

namespace Stratum
{
	public abstract class StratumPlugin : BaseUnityPlugin, IStratumPlugin
	{
		protected StratumPlugin()
		{
			DirectoryInfo root = new(Path.GetDirectoryName(Info.Location)!);
			Directories = new PluginDirectories(root);

			StratumRoot.Inject(this);
		}

		protected PluginDirectories Directories { get; }

		public abstract void OnSetup(IStageContext<Empty> ctx);

		public abstract IEnumerator OnRuntime(IStageContext<IEnumerator> ctx);
	}
}
