using System.Collections;
using System.IO;
using UnityEngine;

namespace Stratum
{
	/// <summary>
	///     A method that consumes a file/directory and produces a side effect
	/// </summary>
	public delegate TRet Loader<out TRet>(FileSystemInfo handle);

	/// <summary>
	///     A method that returns a coroutine handle from coroutine enumerator
	/// </summary>
	public delegate Coroutine CoroutineStarter(IEnumerator coroutine);
}
