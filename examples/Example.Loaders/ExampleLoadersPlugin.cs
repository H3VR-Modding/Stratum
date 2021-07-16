using System;
using System.Collections;
using System.IO;
using BepInEx;
using Stratum;
using Stratum.Extensions;

namespace Example.Loaders
{
	[BepInPlugin("stratum.example.loaders", "Stratum Example (Loaders)", StratumRoot.Version)]
	[BepInDependency(StratumRoot.GUID, StratumRoot.Version)]
	// You MUST add attributes that denote what loaders you add and what stage you add them to.
	// Failure to do this results in an exception that will kill your plugin (and any that depend on it, of course)
	[StratumLoader(Stages.Setup, PrintLoaderName)]
	[StratumLoader(Stages.Runtime, PrintLoaderName)]
	public class ExampleLoadersPlugin : StratumPlugin
	{
		private const string PrintLoaderName = "print";

		#region Readers/Writers

		private static string StringReader(FileInfo file) => File.ReadAllText(file.FullName);

		private static void StringWriter(FileInfo file, string value) => File.WriteAllText(file.FullName, value);

		private static int IntReader(FileInfo file) => int.Parse(StringReader(file));

		private static void IntWriter(FileInfo file, int value) => StringWriter(file, value.ToString());

		private static string[] StringsReader(FileInfo file) => File.ReadAllLines(file.FullName);

		#endregion

		private readonly FileInfo _framesFile;

		private int _frames;

		private event Action? OnDestroyed;

		public ExampleLoadersPlugin()
		{
			_framesFile = Directories.Cache.GetFile("frame.txt");
		}

		public override void OnSetup(IStageContext<Empty> ctx)
		{
			Empty PrintLoader(FileSystemInfo handle)
			{
				// Read a string from the file, which asserts that the handle is a file.
				var message = StringReader(handle.ConsumeFile());
				Logger.LogMessage($"A message was loaded at setup: {message}");

				return new();
			}

			// Add loader Setup/stratum.example.loaders:print
			ctx.Loaders.Add(PrintLoaderName, PrintLoader);
		}

		public override IEnumerator OnRuntime(IStageContext<IEnumerator> ctx)
		{
			InitFrames();
			InitLoader(ctx);

			yield break;
		}

		private void OnDestroy() => OnDestroyed?.Invoke();

		private void InitFrames()
		{
			if (_framesFile.Exists)
			{
				_frames = IntReader(_framesFile);
			}

			void WriteFrames() => IntWriter(_framesFile, _frames);

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

		private void InitLoader(IStageContext<IEnumerator> ctx)
		{
			IEnumerator PrintLoader(FileSystemInfo handle)
			{
				foreach (var message in StringsReader(handle.ConsumeFile()))
				{
					Logger.LogMessage($"A message was loaded at runtime (frame: {_frames}): {message}");
					yield return null;
				}
			}

			// Add loader Runtime/stratum.example.loaders:print
			ctx.Loaders.Add(PrintLoaderName, PrintLoader);
		}
	}
}
