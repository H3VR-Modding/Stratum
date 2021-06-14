using System;

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
