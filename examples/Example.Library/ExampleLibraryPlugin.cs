using System.Collections;
using System.IO;
using System.Threading;
using BepInEx;
using Stratum;
using Stratum.Coroutines;

namespace Example.Library
{
	[BepInPlugin("stratum.example.library", "Stratum Example (Library)", StratumRoot.Version)]
	[BepInDependency(StratumRoot.GUID, StratumRoot.Version)]
	public class ExampleLibraryPlugin : StratumPlugin
	{
		private static string StringReader(FileInfo file) => File.ReadAllText(file.FullName);

		private static Empty StringWriter(FileInfo file, string value)
		{
			File.WriteAllText(file.FullName, value);
			return new();
		}

		private static int IntReader(FileInfo file) => int.Parse(StringReader(file));

		private static Empty IntWriter(FileInfo file, int value) => StringWriter(file, value.ToString());

		private static string[] StringsReader(FileInfo file) => File.ReadAllLines(file.FullName);

		private static IResultEnumerator<byte[]> BytesReader(FileInfo file)
		{
			byte[]? result = null;

			// Queue a file read
			ThreadPool.QueueUserWorkItem(_ => result = File.ReadAllBytes(file.FullName));

			IEnumerator Body(IRet<byte[]> ret)
			{
				// Wait for it to complete
				while (result is null)
					yield return null;

				// Set return value (failure to do this results in an exception)
				ret.Value = result;
			}

			return new WaitForResult<byte[]>(Body);
		}

		// Sometimes, you have readers/writers that ONLY work before the application begins running. We don't have any, so we just add all
		// of our IO to every stage
		private void OnAnything<TRet>(IStageContext<TRet> ctx)
		{
			ctx.Readers.Add(StringReader);
			ctx.Writers.Add<string, Empty>(StringWriter);

			ctx.Readers.Add(IntReader);
			ctx.Writers.Add<int, Empty>(IntWriter);

			ctx.Readers.Add(StringsReader);
		}

		public override void OnSetup(IStageContext<Empty> ctx) => OnAnything(ctx);

		public override IEnumerator OnRuntime(IStageContext<IEnumerator> ctx)
		{
			OnAnything(ctx);

			ctx.Readers.Add(BytesReader);

			yield break;
		}
	}
}
