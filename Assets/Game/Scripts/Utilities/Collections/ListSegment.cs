using System;
using System.Collections;
using System.Collections.Generic;
namespace Game.Utilities.Collections
{
	public readonly struct ListSegment<T> : IReadOnlyList<T>
	{
		public struct Enumerator : IEnumerator<T>
		{
			readonly ListSegment<T> segment;
			int index;
			public readonly T Current => segment.rawList[segment.offset + index];
			readonly object IEnumerator.Current => Current;
			public Enumerator(ListSegment<T> segment)
			{
				this.segment = segment;
				index = -1;
			}
			bool IEnumerator.MoveNext() => ++index < segment.Count;
			void IEnumerator.Reset() => index = -1;
			void IDisposable.Dispose() { }
		}
		readonly IList<T> rawList;
		readonly int offset;
		public int Count { get; }
		T IReadOnlyList<T>.this[int index] => rawList[offset + index];
		public ListSegment(IList<T> rawList, int offset, int count)
		{
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be non-negative.");
			if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be positive.");
			this.rawList = rawList;
			this.offset = offset;
			Count = count;
		}
		public Enumerator GetEnumerator() => new(this);
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
	}
}
