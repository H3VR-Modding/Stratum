using System;

namespace Stratum
{
	/// <summary>
	///		Declares loaders that this plugin provides. If a loader is not provided, an exception is thrown and the plugin is loaded no
	///		further.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public abstract class StratumLoadersAttribute : Attribute
	{
		/// <summary>
		///		Constructs an instance of <see cref="StratumLoadersAttribute"/>
		/// </summary>
		/// <param name="stage">The stage in which the loaders are provided</param>
		/// <param name="names">The names of the loaders provided</param>
		protected StratumLoadersAttribute(Stages stage, string[] names)
		{
			Stage = stage;
			Names = names;
		}

		/// <summary>
		///		The stage in which the loaders are provided
		/// </summary>
		public Stages Stage { get; }

		/// <summary>
		///		The names of the loaders provided
		/// </summary>
		public string[] Names { get; }
	}
}
