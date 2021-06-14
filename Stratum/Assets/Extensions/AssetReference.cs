using System.IO;
using Stratum.Extensions;

namespace Stratum.Assets
{
	public static class ExtAssetReference
	{
		public static AssetDefinition<TRet> Resolve<TRet>(this AssetReference @this, IStage<TRet> stage, DirectoryInfo root)
		{
			var loader = @this.Loader.Resolve(stage);
			var handle = root.GetChild(@this.Path) ?? throw
				new FileNotFoundException($"No file/directory existed at path {@this.Path} (root: {root})");

			return new(handle, loader);
		}
	}
}
