using System;

namespace Stratum
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class StratumLoaderAttribute : Attribute, IEquatable<StratumLoaderAttribute>
	{
		public StratumLoaderAttribute(Stages stage, string name)
		{
			Stage = stage;
			Name = name;
		}

		public Stages Stage { get; }

		public string Name { get; }

		public bool Equals(StratumLoaderAttribute? other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return base.Equals(other) && Stage == other.Stage && Name == other.Name;
		}

		public override bool Equals(object? obj)
		{
			return ReferenceEquals(this, obj) || obj is StratumLoaderAttribute other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int) Stage * 397) ^ Name.GetHashCode();
			}
		}
	}
}
