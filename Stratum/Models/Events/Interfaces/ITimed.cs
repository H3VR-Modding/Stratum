using System;

namespace Stratum
{
	public interface ITimed
	{
		TimeSpan Duration { get; }
	}
}
