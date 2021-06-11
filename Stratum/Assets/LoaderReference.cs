using System;
using System.Collections.Generic;
using Stratum.IO;

namespace Stratum.Assets
{
	public readonly struct LoaderReference : IEquatable<LoaderReference>
	{
		public string GUID { get; }

		public string Name { get; }

		public LoaderReference(string guid, string name)
		{
			GUID = guid;
			Name = name;
		}

		public Loader<TRet> Resolve<TRet>(IStage<TRet> stage)
		{
        	if (!stage[GUID].Loaders.TryGetValue(Name, out var resolved))
        		throw new KeyNotFoundException($"The plugin with GUID '{GUID}' did not have the loader named '{Name}'");

            return resolved;
		}

		public bool Equals(LoaderReference other) => GUID == other.GUID && Name == other.Name;

		public override bool Equals(object? obj) => obj is LoaderReference other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				return (GUID.GetHashCode() * 397) ^ Name.GetHashCode();
			}
		}

		public override string ToString() => $"{GUID}::{Name}";
	}
}
