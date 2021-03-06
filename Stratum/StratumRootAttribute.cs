// ------------------------------------------------------------
// This C# file was generated from a T4 template file.
// Edits to this file will be overwritten by subsequent builds.
// Edit the template file directory.
// ------------------------------------------------------------

using BepInEx;

namespace Stratum
{
    [BepInPlugin(GUID, Name, "1.1.1")]
	public sealed partial class StratumRoot
	{
		// People can use 'Version' in their 'BepInDependency's, but not 'ExactVersion', because there is no good reason to depend on the
		// patch component.
		// 'Version' should be intentionally rough to prevent people from baking in the full Stratum version in other places too. If a
        // plugin is built on 1.0.0, and 1.0.1 releases, the plugin would still think its 1.0.0 because 'Version' is a constant.
		/// <summary>
		///     The version of Stratum with a zeroed patch component. To find the exact version, use the BepInEx API
		/// </summary>
		public const string Version = "1.1.0";
	}
}
