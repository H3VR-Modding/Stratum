using System.Collections;
using BepInEx;
using UnityEngine;

namespace Stratum
{
	/// <summary>
	///     Represents a plugin within Stratum. This is the read-only version of <see cref="IStratumPlugin" />.
	/// </summary>
	public interface IReadOnlyStratumPlugin
	{
		/// <summary>
		///     Metadata about the BepInEx plugin that owns this Stratum plugin
		/// </summary>
		PluginInfo Info { get; }
	}

	/// <summary>
	///     Represents a plugin that can be inserted into Stratum. Unless you are inheriting from another <see cref="MonoBehaviour" />, you
	///     should inherit from <see cref="StratumPlugin" />, which implements this interface for you.
	/// </summary>
	public interface IStratumPlugin : IReadOnlyStratumPlugin
	{
		/// <summary>
		///     Called once all plugins are loaded. The entire stage spans a singular frame.
		/// </summary>
		/// <param name="ctx">Plugin-specific information relevant to this stage</param>
		void OnSetup(IStageContext<Empty> ctx);

		/// <summary>
		///     Called after <see cref="OnSetup" />. This method is a Unity coroutine, and may span multiple frames (as can the stage).
		/// </summary>
		/// <param name="ctx">Plugin-specific information relevant to this stage</param>
		/// <returns>A Unity coroutine</returns>
		IEnumerator OnRuntime(IStageContext<IEnumerator> ctx);
	}
}
