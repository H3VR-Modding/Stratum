using System.Collections.Generic;
using System.Linq;

namespace Stratum.Internal
{
	internal class Plugin
	{
		private Dictionary<Stages, string[]>? _loaders;

		public Plugin(IStratumPlugin content)
		{
			Content = content;
		}

		public IStratumPlugin Content { get; }

		public Dictionary<Stages, string[]> Loaders => _loaders ??= Content.GetType()
			.GetCustomAttributes<StratumLoadersAttribute>()
			.ToDictionary(x => x.Stage, x => x.Names);

		public override string ToString()
		{
			return Content.Info.ToString();
		}
	}
}
