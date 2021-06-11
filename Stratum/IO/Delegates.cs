using System.IO;

namespace Stratum.IO
{
	public delegate TRet Loader<out TRet>(FileSystemInfo handle);

	public delegate T Reader<out T>(FileInfo file);

	public delegate TRet Writer<in T, out TRet>(FileInfo file, T contents);
}
