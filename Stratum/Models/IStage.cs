using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///		Represents a batch of <see cref="IStratumPlugin"/>s
	/// </summary>
	public interface IStage<TRet> : IEnumerable<IReadOnlyStageContext<TRet>>
	{
		/// <summary>
		///		The discrete stage type
		/// </summary>
		Stages Variant { get; }

		/// <summary>
		///		Gets the stage context of an already-called plugin
		/// </summary>
		/// <param name="guid">The GUID of the plugin</param>
		IReadOnlyStageContext<TRet> this[string guid] { get; }

		/// <summary>
		///		Tries to get the stage context of an already-called plugin
		/// </summary>
		/// <param name="guid">The GUID of the plugin</param>
		/// <returns>The plugin's context if found, otherwise null</returns>
		IReadOnlyStageContext<TRet>? TryGet(string guid);
	}
}
