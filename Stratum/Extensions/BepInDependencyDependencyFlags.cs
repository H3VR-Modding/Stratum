using BepInEx;

namespace Stratum.Extensions
{
	/// <summary>
	///     Extension methods pertaining to <see cref="BepInDependency.DependencyFlags" />
	/// </summary>
	public static class ExtBepInDependencyDependencyFlags
	{
		/// <summary>
		///     Checks if this has a flag enabled
		/// </summary>
		/// <param name="this"></param>
		/// <param name="flag">The flag to check</param>
		public static bool HasFlag(this BepInDependency.DependencyFlags @this, BepInDependency.DependencyFlags flag)
		{
			return (@this & flag) == flag;
		}
	}
}
