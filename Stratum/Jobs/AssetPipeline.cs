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
				IReadOnlyStageContext<TRet> ctx = stage[plugin];

				if (!ctx.Loaders.TryGetValue(name, out Loader<TRet> loader))
					throw new KeyNotFoundException($"The plugin '{plugin}' did not have the loader named '{name}'");

				FileSystemInfo handle = Root.GetChild(path) ?? throw
					new FileNotFoundException($"No file/directory existed at path {path} (root: {Root}). Maybe the resource was deleted or renamed?");

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
			return new AssetPipeline<TRet>(this, Root);
		}
	}
}
