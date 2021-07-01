using System;
using System.IO;
using Stratum.Extensions;

namespace Stratum.Assets
{
	public readonly struct AssetReference : IEquatable<AssetReference>
	{
		public string Path { get; }

		public LoaderReference Loader { get; }

		public AssetReference(string path, LoaderReference loader)
		{
			Path = path;
			Loader = loader;
		}

		public AssetDefinition<TRet> Resolve<TRet>(IStage<TRet> stage, DirectoryInfo root)
		{
			var loader = Loader.Resolve(stage);
			var handle = root.GetChild(Path) ?? throw
				new FileNotFoundException($"No file/directory existed at path {Path} (root: {root})");

			return new(handle, loader);
		}

		public bool Equals(AssetReference other) => Path == other.Path && Loader.Equals(other.Loader);

		public override bool Equals(object? obj) => obj is AssetReference other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				return (Path.GetHashCode() * 397) ^ Loader.GetHashCode();
			}
		}

		public override string ToString() => $"[{Loader} <- '{Path}']";
	}
}
