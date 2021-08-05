using BepInEx;
using Stratum.Extensions;
using Xunit;

namespace Stratum.Tests.Extensions
{
	public class ExtBepInDependencyDependencyFlagsTests
	{
		[Theory]
		[InlineData(~default(BepInDependency.DependencyFlags), BepInDependency.DependencyFlags.HardDependency, true)]
		[InlineData(~default(BepInDependency.DependencyFlags), BepInDependency.DependencyFlags.SoftDependency, true)]
		[InlineData(default(BepInDependency.DependencyFlags), BepInDependency.DependencyFlags.HardDependency, false)]
		[InlineData(default(BepInDependency.DependencyFlags), BepInDependency.DependencyFlags.SoftDependency, false)]
		public void HasFlag(BepInDependency.DependencyFlags value, BepInDependency.DependencyFlags flag, bool result)
		{
			bool ret = value.HasFlagFast(flag);

			Assert.Equal(result, ret);
		}
	}
}
