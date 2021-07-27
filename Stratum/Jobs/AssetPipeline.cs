using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Stratum.Extensions;

namespace Stratum.Jobs
{
	/// <summary>
	///     A collection of jobs to be ran, which includes assets
	/// </summary>
	public abstract class AssetPipeline<TRet, TSelf> : Pipeline<TRet, TSelf> where TSelf : Pipeline<TRet, TSelf>
	{
		/// <summary>
		///     Constructs an instance of <see cref="AssetPipeline{TRet}" />
		/// </summary>
		/// <param name="parent">The pipeline that will execute this pipeline</param>
		/// <param name="root">The root directory of all assets added by this pipeline</param>
		protected AssetPipeline(TSelf parent, DirectoryInfo root) : base(parent)
		{
			Root = root;
		}

		/// <summary>
		///     Constructs an instance of <see cref="AssetPipeline{TRet}" />
		/// </summary>
		/// <param name="root">The root directory of all assets added by this pipeline</param>
		protected AssetPipeline(DirectoryInfo root)
		{
			Root = root;
		}

		/// <summary>
		///     The root directory of all assets added by this pipeline
		/// </summary>
		public DirectoryInfo Root { get; }

		/// <summary>
		///     Adds an asset, which will be executed as a job
		/// </summary>
		/// <param name="path">The path to the asset (relative to <see cref="Root" /></param>
		/// <param name="plugin">The GUID of the loader's plugin</param>
		/// <param name="name">The name of the loader</param>
		public TSelf AddAsset(string plugin, string name, string path)
		{
			TRet Job(IStage<TRet> stage, ManualLogSource logger)
			{
				string fmt = ToString();
				string pad = new(' ', fmt.Length);

				logger.LogDebug($"{fmt} | Resolving plugin '{plugin}'");
				IReadOnlyStageContext<TRet> ctx = stage[plugin];

				logger.LogDebug($"{pad} | Resolving loader '{name}'");
				if (!ctx.Loaders.TryGetValue(plugin, out Loader<TRet> loader))
					throw new KeyNotFoundException($"The plugin '{plugin}' did not have the loader named '{name}'");

				logger.LogDebug($"{pad} | Resolving resource '{path}'");
				FileSystemInfo handle = Root.GetChild(path) ?? throw
					new FileNotFoundException($"No file/directory existed at path {path} (root: {Root})");

				logger.LogDebug($"{pad} | Running");
				return loader(handle);
			}

			return AddJob(Job);
		}
	}

	/// <summary>
	///     A collection of jobs to be ran, which includes assets
	/// </summary>
	public sealed class AssetPipeline<TRet> : AssetPipeline<TRet, AssetPipeline<TRet>>
	{
		private AssetPipeline(AssetPipeline<TRet> parent, DirectoryInfo root) : base(parent, root) { }

		/// <inheritdoc cref="AssetPipeline{TRet,TSelf}(System.IO.DirectoryInfo)" />
		public AssetPipeline(DirectoryInfo root) : base(root) { }

		/// <inheritdoc cref="Pipeline{TRet,TSelf}.CreateNested" />
		protected override AssetPipeline<TRet> CreateNested()
		{
			return new(this, Root);
		}
	}
}
