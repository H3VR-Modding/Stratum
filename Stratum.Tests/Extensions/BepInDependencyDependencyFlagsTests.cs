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
		public void HasFlagFast(BepInDependency.DependencyFlags value, BepInDependency.DependencyFlags flag, bool result)
		{
			bool ret = value.HasFlagFast(flag);

			Assert.Equal(result, ret);
		}

		[Theory]
		[InlineData(~default(BepInDependency.DependencyFlags), BepInDependency.DependencyFlags.HardDependency)]
		[InlineData(~default(BepInDependency.DependencyFlags), BepInDependency.DependencyFlags.SoftDependency)]
		[InlineData(default(BepInDependency.DependencyFlags), BepInDependency.DependencyFlags.HardDependency)]
		[InlineData(default(BepInDependency.DependencyFlags), BepInDependency.DependencyFlags.SoftDependency)]
		public void HasFlag_IsSameAsFast(BepInDependency.DependencyFlags value, BepInDependency.DependencyFlags flag)
		{
			bool expected = value.HasFlagFast(flag);
#pragma warning disable 618
			bool obsolete = ExtBepInDependencyDependencyFlags.HasFlag(value, flag);
#pragma warning restore 618

			Assert.Equal(expected, obsolete);
		}
	}
}
