using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Stratum.Internal;

namespace Stratum
{
	/// <summary>
	///     The Stratum BepInEx plugin
	/// </summary>
	[BepInPlugin(GUID, "Stratum", ExactVersion)]
	public sealed class StratumRoot : BaseUnityPlugin
	{
		private const string MajorMinorVersion = "1.0.";

		// This is 'Version' but with the patch component not zeroed
		// People can use 'Version' in their 'BepInDependency's, but not 'ExactVersion', because there is no good reason to depend on the
		// patch component.
		// 'Version' should be intentionally rough to prevent people from baking in the full Stratum version in places other than the
		// dependency attribute too. If a plugin is built on 1.0.0, and 1.0.1 releases, the plugin would still think its 1.0.0 because
		// 'Version' is a constant.
		private const string ExactVersion = MajorMinorVersion + "1";

		/// <summary>
		///     The BepInEx GUID used by Stratum
		/// </summary>
		public const string GUID = "stratum";

		/// <summary>
		///     The version of Stratum with a zeroed patch component. To find the exact version, use the BepInEx API
		/// </summary>
		public const string Version = MajorMinorVersion + "0";

		private static StratumRoot? _instance;

		/// <summary>
		///     Adds a plugin to Stratum, allowing it to utilize all the benefits of the Stratum ecosystem
		/// </summary>
		/// <param name="plugin">The plugin to add to Stratum</param>
		/// <exception cref="InvalidOperationException">Stratum has not yet initialised or it has already started</exception>
		public static void Inject(IStratumPlugin plugin)
		{
			if (_instance == null)
				throw new InvalidOperationException(
					"Stratum has not yet initialised! Please ensure your BepInEx plugin depends on 'stratum'.");

			_instance.InjectInstance(plugin);
		}

		private readonly List<IStratumPlugin> _plugins = new();

		private bool _started;

		private void Awake()
		{
			_instance = this;

			ChainloaderCompleteHook.Create(Load, StartCoroutine);
		}

		private void Load()
		{
			_started = true;

			// ggez
			if (_plugins.Count == 0)
				return;

			Bootstrap.Run(Logger, _plugins, StartCoroutine);
		}

		private void InjectInstance(IStratumPlugin plugin)
		{
			// BepInEx.ScriptEngine
			if (_started)
				throw new InvalidOperationException("Plugins cannot be injected after stage execution begins. This is likely " +
				                                    "from the plugin being reloaded via BepInEx.ScriptEngine.");

			// Avoid duplicating any plugins
			if (_plugins.Contains(plugin))
			{
				Logger.LogWarning($"{plugin.Info} attempted to inject itself again.");
				return;
			}

			_plugins.Add(plugin);
		}
	}
}
