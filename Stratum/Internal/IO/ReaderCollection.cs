using System.Diagnostics.CodeAnalysis;
using Stratum.IO;

namespace Stratum.Internal.IO
{
	// Type aliasing moment
	internal class ReaderCollection : AliasedSingletonCollection, IReaderCollection
	{
		public void Add<T>(Reader<T> reader) => Write.Add(reader);

		public bool Contains<T>() => Read.Contains<Reader<T>>();

		public Reader<T> Get<T>() => Read.Get<Reader<T>>()!;

		public bool TryGet<T>([MaybeNullWhen(false)] out Reader<T> reader) => Read.TryGet(out reader);
	}
}
