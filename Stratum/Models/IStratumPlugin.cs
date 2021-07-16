using System.Collections;
using BepInEx;

namespace Stratum
{
	public interface IReadOnlyStratumPlugin
	{
		PluginInfo Info { get; }
	}

	public interface IStratumPlugin : IReadOnlyStratumPlugin
	{
		void OnSetup(IStageContext<Empty> ctx);

		IEnumerator OnRuntime(IStageContext<IEnumerator> ctx);
	}
}
