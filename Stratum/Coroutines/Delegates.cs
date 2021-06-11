using System.Collections;
using UnityEngine;

namespace Stratum.Coroutines
{
	public delegate IEnumerator ResultEnumeratorBody<out T>(IRet<T> ret);

	public delegate Coroutine CoroutineStarter(IEnumerator coroutine);
}
