// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;

namespace ICSharpCode.TextEditor.Util
{
	/// <summary>
	///     A collection that does not allows its elements to be garbage-collected (unless there are other
	///     references to the elements). Elements will disappear from the collection when they are
	///     garbage-collected.
	///     The WeakCollection is not thread-safe, not even for read-only access!
	///     No methods may be called on the WeakCollection while it is enumerated, not even a Contains or
	///     creating a second enumerator.
	///     The WeakCollection does not preserve any order among its contents; the ordering may be different each
	///     time the collection is enumerated.
	///     Since items may disappear at any time when they are garbage collected, this class
	///     cannot provide a useful implementation for Count and thus cannot implement the ICollection interface.
	/// </summary>
	public class WeakCollection<T> : IEnumerable<T> where T : class
	{
		private readonly List<WeakReference> _innerList = new List<WeakReference>();

		private bool _hasEnumerator;

		/// <summary>
		///     Enumerates the collection.
		///     Each MoveNext() call on the enumerator is O(1), thus the enumeration is O(n).
		/// </summary>
		public IEnumerator<T> GetEnumerator()
		{
			if (_hasEnumerator)
				throw new InvalidOperationException(
					"The WeakCollection is already being enumerated, it cannot be enumerated twice at the same time. Ensure you dispose the first enumerator before using another enumerator.");
			try
			{
				_hasEnumerator = true;
				for (var i = 0; i < _innerList.Count;)
				{
					var element = (T) _innerList[i].Target;
					if (element == null)
					{
						RemoveAt(i);
					}
					else
					{
						yield return element;
						i++;
					}
				}
			}
			finally
			{
				_hasEnumerator = false;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		///     Adds an element to the collection. Runtime: O(n).
		/// </summary>
		public void Add(T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			CheckNoEnumerator();
			if (_innerList.Count == _innerList.Capacity || _innerList.Count % 32 == 31)
				_innerList.RemoveAll(delegate(WeakReference r) { return !r.IsAlive; });
			_innerList.Add(new WeakReference(item));
		}

		/// <summary>
		///     Removes all elements from the collection. Runtime: O(n).
		/// </summary>
		public void Clear()
		{
			_innerList.Clear();
			CheckNoEnumerator();
		}

		/// <summary>
		///     Checks if the collection contains an item. Runtime: O(n).
		/// </summary>
		public bool Contains(T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			CheckNoEnumerator();
			foreach (var element in this)
				if (item.Equals(element))
					return true;
			return false;
		}

		/// <summary>
		///     Removes an element from the collection. Returns true if the item is found and removed,
		///     false when the item is not found.
		///     Runtime: O(n).
		/// </summary>
		public bool Remove(T item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			CheckNoEnumerator();
			for (var i = 0; i < _innerList.Count;)
			{
				var element = (T) _innerList[i].Target;
				if (element == null)
				{
					RemoveAt(i);
				}
				else if (element == item)
				{
					RemoveAt(i);
					return true;
				}
				else
				{
					i++;
				}
			}
			return false;
		}

		private void RemoveAt(int i)
		{
			var lastIndex = _innerList.Count - 1;
			_innerList[i] = _innerList[lastIndex];
			_innerList.RemoveAt(lastIndex);
		}

		private void CheckNoEnumerator()
		{
			if (_hasEnumerator)
				throw new InvalidOperationException(
					"The WeakCollection is already being enumerated, it cannot be modified at the same time. Ensure you dispose the first enumerator before modifying the WeakCollection.");
		}
	}
}