using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Stratum.IO
{
	public interface IReadOnlyWriterCollection : ICollection
	{
		bool Contains<T, TRet>();

		Writer<T, TRet> Get<T, TRet>();

		bool TryGet<T, TRet>([MaybeNullWhen(false)] out Writer<T, TRet> reader);
	}

	public interface IWriterCollection : IReadOnlyWriterCollection
	{
		void Add<T, TRet>(Writer<T, TRet> writer);
	}
}
