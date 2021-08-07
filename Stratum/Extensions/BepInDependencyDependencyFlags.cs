using System;
using BepInEx;

namespace Stratum.Extensions
{
	/// <summary>
	///     Extension methods pertaining to <see cref="BepInDependency.DependencyFlags" />
	/// </summary>
	public static class ExtBepInDependencyDependencyFlags
	{
		/// <inheritdoc cref="HasFlagFast"/>
		[Obsolete("Use HasFlagFast")]
		public static bool HasFlag(this BepInDependency.DependencyFlags @this, BepInDependency.DependencyFlags flag)
		{
			return @this.HasFlagFast(flag);
		}

		/// <summary>
		///     Checks if this has a flag enabled
		/// </summary>
		/// <param name="this"></param>
		/// <param name="flag">The flag to check</param>
		public static bool HasFlagFast(this BepInDependency.DependencyFlags @this, BepInDependency.DependencyFlags flag)
		{
			return (@this & flag) == flag;
		}
	}
}
