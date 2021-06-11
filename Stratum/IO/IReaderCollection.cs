using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Stratum.IO
{
	public interface IReadOnlyReaderCollection : ICollection
	{
		bool Contains<T>();

		Reader<T> Get<T>();

		bool TryGet<T>([MaybeNullWhen(false)] out Reader<T> reader);
	}

	public interface IReaderCollection : IReadOnlyReaderCollection
	{
		public void Add<T>(Reader<T> reader);
	}
}
