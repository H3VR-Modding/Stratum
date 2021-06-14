using System.Collections.Generic;
using Stratum.IO;

namespace Stratum.Assets
{
	public static class ExtLoaderReference
	{
		public static Loader<TRet> Resolve<TRet>(this LoaderReference @this, IStage<TRet> stage)
		{
			if (!stage[@this.Plugin].Loaders.TryGetValue(@this.Name, out var resolved))
				throw new KeyNotFoundException($"The plugin with GUID '{@this.Plugin}' did not have the loader named '{@this.Name}'");

			return resolved;
		}
	}
}
