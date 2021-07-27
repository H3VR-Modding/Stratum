using System;

namespace Stratum
{
	/// <summary>
	///     The discrete stages of Stratum
	/// </summary>
	public enum Stages : byte
	{
		/// <summary>
		///     The setup stage. This stage is ran entirely within a single frame.
		/// </summary>
		Setup,

		/// <summary>
		///     The runtime stage. This stage is ran across multiple frames, in which each call is an awaitable Unity coroutine
		/// </summary>
		Runtime
	}

	/// <summary>
	///     Extension methods pertaining to <see cref="Stages" />
	/// </summary>
	public static class ExtStages
	{
		/// <summary>
		///     Converts a stage to a human-friendly string
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Value was not a valid stage</exception>
		public static string ToFriendlyString(this Stages @this)
		{
			return @this switch
			{
				Stages.Setup => "setup",
				Stages.Runtime => "runtime",
				_ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null)
			};
		}
	}
}
