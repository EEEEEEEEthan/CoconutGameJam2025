using System;
namespace Game.Utilities.Collections
{
	public interface IReadOnlyHeap<TKey, TValue> where TValue : IComparable<TValue>
	{
		int Count { get; }
		TKey Peek(out TValue value);
		TKey Peek();
		bool TryPeek(out TKey key);
		bool TryPeek(out TKey key, out TValue value);
	}
	public interface IHeap<T, TValue> : IReadOnlyHeap<T, TValue> where TValue : IComparable<TValue>
	{
		void Add(T element, TValue sortingValue);
		T Pop();
		T Pop(out TValue value);
		bool TryPop(out T key, out TValue value);
		void TrimExcess();
	}
}
