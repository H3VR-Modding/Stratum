using System;
using System.Collections;
using System.IO;
using BepInEx;
using Stratum;
using Stratum.Extensions;
using Stratum.IO;

namespace Example.Loaders
{
	[BepInPlugin("stratum.example.loaders", "Stratum Example (Loaders)", StratumRoot.Version)]
	[BepInDependency(StratumRoot.GUID, StratumRoot.Version)]
	[BepInDependency(LibraryGUID, StratumRoot.Version)]
	// You MUST add attributes that denote what loaders you add and what stage you add them to.
	// Failure to do this results in an exception that will kill your mod (and any that depend on it, of course)
	[StratumLoader(Stages.Setup, PrintLoaderName)]
	[StratumLoader(Stages.Runtime, PrintLoaderName)]
	public class ExampleLoadersPlugin : StratumPlugin
	{
		private const string LibraryGUID = "stratum.example.library";
		private const string PrintLoaderName = "print";

		private readonly FileInfo _framesFile;

		private int _frames;

		private event Action? OnDestroyed;

		public ExampleLoadersPlugin()
		{
			_framesFile = Directories.Cache.GetFile("frame.txt");
		}

		public override void OnSetup(IStageContext<Empty> ctx)
		{
			var lib = ctx.Stage[LibraryGUID];

			var stringReader = lib.Readers.Get<string>();
			Empty PrintLoader(FileSystemInfo handle)
			{
				// Read a string from the file, which asserts that the handle is a file.
				var message = stringReader(handle.RequireFile());
				Logger.LogMessage($"A message was loaded at setup: {message}");

				return new();
			}

			// Add loader Setup/stratum.example.loaders::print
			ctx.Loaders.Add(PrintLoaderName, PrintLoader);
		}

		private void InitFrames(IReadOnlyStageContext<IEnumerator> lib)
		{
			var intWriter = lib.Writers.Get<int, Empty>();

			if (_framesFile.Exists)
			{
				var intReader = lib.Readers.Get<int>();

				_frames = intReader(_framesFile);
			}

			void WriteFrames() => intWriter(_framesFile, _frames);

			IEnumerator IterateFrames()
			{
				const int interval = 3000;

				while (true)
				{
					var next = _frames + interval;
					for (; _frames < next; ++_frames)
						yield return null;

					WriteFrames();
				}
			}

			StartCoroutine(IterateFrames());
			OnDestroyed += WriteFrames;
		}

		private void InitLoader(IStageContext<IEnumerator> ctx, IReadOnlyStageContext<IEnumerator> lib)
		{
			var multilineReader = lib.Readers.Get<string[]>();

			IEnumerator PrintLoader(FileSystemInfo handle)
			{
				foreach (var message in multilineReader(handle.RequireFile()))
				{
					Logger.LogMessage($"A message was loaded at runtime (frame: {_frames}): {message}");
					yield return null;
				}
			}

			// Add loader Runtime/stratum.example.loaders::print
			ctx.Loaders.Add(PrintLoaderName, PrintLoader);
		}

		public override IEnumerator OnRuntime(IStageContext<IEnumerator> ctx)
		{
			var lib = ctx.Stage[LibraryGUID];

			InitFrames(lib);
			InitLoader(ctx, lib);

			yield break;
		}

		private void OnDestroy() => OnDestroyed?.Invoke();
	}
}
