using System.Diagnostics.CodeAnalysis;
using Stratum.IO;

namespace Stratum.Internal.IO
{
	// Another type aliasing moment
	internal class WriterCollection : AliasedSingletonCollection, IWriterCollection
	{
		public void Add<T, TRet>(Writer<T, TRet> writer) => Write.Add(writer);

		public bool Contains<T, TRet>() => Read.Contains<Writer<T, TRet>>();

		public Writer<T, TRet> Get<T, TRet>() => Read.Get<Writer<T, TRet>>()!;

		public bool TryGet<T, TRet>([MaybeNullWhen(false)] out Writer<T, TRet> writer) => Read.TryGet(out writer);
	}
}
