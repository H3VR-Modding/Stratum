using System;

namespace Stratum
{
	/// <summary>
	///     Represents an operation whose duration, from start to finish, was recorded
	/// </summary>
	public interface ITimed
	{
		/// <summary>
		///     The time it took to complete the operation
		/// </summary>
		TimeSpan Duration { get; }
	}
}
