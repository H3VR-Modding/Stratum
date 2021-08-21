using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///		Represents event data for a batch
	/// </summary>
	/// <typeparam name="TPlugin">The type that represents a plugin</typeparam>
	public interface IEventBatch<TPlugin> : IHasStage
	{
		/// <summary>
		///		The number of batches loaded before this batch
		/// </summary>
		int Generation { get; }

		/// <summary>
		///		All plugins loading or loaded in the batch
		/// </summary>
		IReadOnlyList<TPlugin> Plugins { get; }
	}
}
