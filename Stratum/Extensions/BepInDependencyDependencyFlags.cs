using BepInEx;

namespace Stratum.Extensions
{
	public static class ExtBepInDependencyDependencyFlags
	{
		public static bool HasFlag(this BepInDependency.DependencyFlags @this, BepInDependency.DependencyFlags flag)
		{
			return (@this & flag) == flag;
		}
	}
}
