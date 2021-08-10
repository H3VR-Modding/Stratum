using System.Collections;
using System.Collections.Generic;

namespace Stratum
{
	public class ReadOnlyList<T> : IReadOnlyList<T>
	{
		public ReadOnlyList(IList<T> list)
		{
			_list = list;
		}

		private readonly IList<T> _list;

		public int Count => _list.Count;

		public T this[int index] => _list[index];

		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _list).GetEnumerator();
		}
	}
}
