using System.Collections;
using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///		An object that implements <see cref="IReadOnlyList{T}"/>
	/// </summary>
	/// <typeparam name="T">The type of the items</typeparam>
	public sealed class ReadOnlyList<T> : IReadOnlyList<T>
	{
		/// <summary>
		///		Constructs an instance of <see cref="ReadOnlyList{T}"/>
		/// </summary>
		/// <param name="list">The list to read from</param>
		public ReadOnlyList(IList<T> list)
		{
			_list = list;
		}

		private readonly IList<T> _list;

		/// <inheritdoc cref="IReadOnlyCollection{T}.Count"/>
		public int Count => _list.Count;

		/// <inheritdoc cref="IReadOnlyList{T}.this"/>
		public T this[int index] => _list[index];

		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
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
