using System;

namespace Stratum.Assets
{
	public readonly struct LoaderReference : IEquatable<LoaderReference>
	{
		public static LoaderReference Parse(string raw)
		{
			var split = raw.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
			if (split.Length != 2)
				throw new FormatException("Loader references must be the mod GUID and loader name, seperated by a double colon (::).");

			return new(split[0], split[1]);
		}

		public string Plugin { get; }

		public string Name { get; }

		public LoaderReference(string plugin, string name)
		{
			Plugin = plugin;
			Name = name;
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

		public override string ToString() => $"{Plugin}::{Name}";
	}
}
