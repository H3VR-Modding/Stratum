namespace Stratum
{
	/// <summary>
	///		Declares runtime loaders that this plugin provides. If a runtime loader is not provided, an exception is thrown and the plugin
	///		is loaded no further.
	/// </summary>
	public sealed class StratumRuntimeLoadersAttribute : StratumLoadersAttribute
	{
		/// <summary>
		///		Constructs an instance of <see cref="StratumRuntimeLoadersAttribute"/>
		/// </summary>
		/// <param name="names">The names of the runtime loaders provided</param>
		public StratumRuntimeLoadersAttribute(params string[] names) : base(Stages.Setup, names) { }
	}
}
