using System.Collections;
using System.IO;
using BepInEx;

namespace Stratum
{
	/// <summary>
	///     A simple implementation of <see cref="IStratumPlugin" />, ready to be inherited
	/// </summary>
	public abstract class StratumPlugin : BaseUnityPlugin, IStratumPlugin
	{
		/// <summary>
		///     Constructs an instance of <see cref="StratumPlugin" />
		/// </summary>
		protected StratumPlugin()
		{
			DirectoryInfo root = new(Path.GetDirectoryName(Info.Location)!);
			Directories = new PluginDirectories(root);

			StratumRoot.Inject(this);
		}

		/// <summary>
		///     The basic directories that this plugin has control over. Beware: there is no collision detection; another plugin can use the
		///     same directories.
		/// </summary>
		protected PluginDirectories Directories { get; }

		/// <inheritdoc cref="IStratumPlugin.OnSetup" />
		public abstract void OnSetup(IStageContext<Empty> ctx);

		/// <inheritdoc cref="IStratumPlugin.OnRuntime" />
		public abstract IEnumerator OnRuntime(IStageContext<IEnumerator> ctx);
	}
}
