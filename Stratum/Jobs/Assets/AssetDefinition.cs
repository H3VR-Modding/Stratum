using System;
using System.IO;

namespace Stratum.Jobs
{
	public readonly struct AssetDefinition<TRet> : IEquatable<AssetDefinition<TRet>>
	{
		public FileSystemInfo Handle { get; }

		public Loader<TRet> Loader { get; }

		public AssetDefinition(FileSystemInfo handle, Loader<TRet> loader)
		{
			Handle = handle;
			Loader = loader;
		}

		public TRet Run()
		{
			return Loader(Handle);
		}

		public bool Equals(AssetDefinition<TRet> other)
		{
			return Handle.Equals(other.Handle) && Loader.Equals(other.Loader);
		}

		public override bool Equals(object? obj)
		{
			return obj is AssetDefinition<TRet> other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Handle.GetHashCode() * 397) ^ Loader.GetHashCode();
			}
		}

		public override string ToString()
		{
			return $"[<{typeof(TRet)}> '{Handle}']";
		}
	}
}
