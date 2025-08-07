using System;
using System.Collections;
using System.Collections.Generic;
using Game.Utilities.Pools;
namespace Game.Utilities
{
	public sealed class RandomSequence : IEnumerator<int>
	{
		static readonly Random random = new();
		readonly List<int> list;
		readonly IDisposable disposable;
		readonly int length;
		int index = -1;
		bool disposed;
		public int Current => list[index];
		object IEnumerator.Current => Current;
		public RandomSequence(int length)
		{
			this.length = length;
			disposable = ListPoolThreaded<int>.Rent(out list);
			for (var i = 0; i < length; ++i) list.Add(i);
		}
		public RandomSequence GetEnumerator() => this;
		public bool MoveNext()
		{
			if (disposed) throw new ObjectDisposedException(nameof(RandomSequence));
			++index;
			if (index >= length) return false;
			var i = random.Next(index, length);
			(list[index], list[i]) = (list[i], list[index]);
			return true;
		}
		public void Reset() => index = -1;
		void IDisposable.Dispose() => index = -1;
		~RandomSequence()
		{
			if (disposed) return;
			disposed = true;
			disposable.Dispose();
		}
	}
}
