using System.Collections;
using System.IO;
using UnityEngine;

namespace Stratum
{
	public delegate TRet Loader<out TRet>(FileSystemInfo handle);

	public delegate Coroutine CoroutineStarter(IEnumerator coroutine);
}
