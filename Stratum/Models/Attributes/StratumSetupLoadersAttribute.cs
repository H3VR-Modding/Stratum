namespace Stratum
{
	/// <summary>
	///		Declares setup loaders that this plugin provides. If a setup loader is not provided, an exception is thrown and the plugin is
	///		loaded no further.
	/// </summary>
	public sealed class StratumSetupLoadersAttribute : StratumLoadersAttribute
	{
		/// <summary>
		///		Constructs an instance of <see cref="StratumSetupLoadersAttribute"/>
		/// </summary>
		/// <param name="names">The names of the setup loaders provided</param>
		public StratumSetupLoadersAttribute(params string[] names) : base(Stages.Setup, names) { }
	}
}
