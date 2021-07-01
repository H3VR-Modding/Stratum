using System;
using System.Collections.Generic;
using Stratum.IO;

namespace Stratum.Assets
{
	public readonly struct LoaderReference : IEquatable<LoaderReference>
	{
		public string Plugin { get; }

		public string Name { get; }

		public LoaderReference(string plugin, string name)
		{
			Plugin = plugin;
			Name = name;
		}

		public Loader<TRet> Resolve<TRet>(IStage<TRet> stage)
		{
			if (!stage[Plugin].Loaders.TryGetValue(Name, out var resolved))
				throw new KeyNotFoundException($"The plugin with GUID '{Plugin}' did not have the loader named '{Name}'");

			return resolved;
		}

		public bool Equals(LoaderReference other) => Plugin == other.Plugin && Name == other.Name;

		public override bool Equals(object? obj) => obj is LoaderReference other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				return (Plugin.GetHashCode() * 397) ^ Name.GetHashCode();
			}
		}

		public override string ToString() => $"{Plugin}:{Name}";
	}
}