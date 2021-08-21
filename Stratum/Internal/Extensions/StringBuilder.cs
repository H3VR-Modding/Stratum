using System.Text;

namespace Stratum.Internal.Extensions
{
	internal static class ExtStringBuilder
	{
		public static StringBuilder AppendExt<T>(this StringBuilder @this, IEventBatch<T> batch)
		{
			return @this
				.Append(batch.Stage.ToFriendlyString())
				.Append(" batch #")
				.Append(batch.Generation + 1)
				.Append(" with ")
				.Append(batch.Plugins.Count)
				.Append(" plugins");
		}

		public static StringBuilder AppendExt(this StringBuilder @this, ITimed timed)
		{
			return @this.AppendFormat("{0:F3}", timed.Duration.TotalSeconds).Append('s');
		}

		public static StringBuilder AppendExt(this StringBuilder @this, IReadOnlyStratumPlugin plugin)
		{
			return @this.Append(plugin.Info);
		}

		public static StringBuilder AppendExt(this StringBuilder @this, Stages stage)
		{
			return @this.Append(stage.ToFriendlyString());
		}
	}
}
