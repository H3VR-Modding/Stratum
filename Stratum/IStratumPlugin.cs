using System.Collections;
using System.Collections.Generic;
using BepInEx;
using Stratum.IO;

namespace Stratum
{
	public interface IReadOnlyStratumPlugin
	{
		PluginInfo Info { get; }

		IEnumerable<StratumLoaderAttribute> Loaders { get; }
	}

	public interface IStratumPlugin : IReadOnlyStratumPlugin
	{
		void OnSetup(IStageContext<Empty> ctx);

		IEnumerator OnRuntime(IStageContext<IEnumerator> ctx);
	}
}
