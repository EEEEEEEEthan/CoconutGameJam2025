using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Game.Utilities.Collections
{
	public sealed class HeapDouble<TKey> : IHeap<TKey, double>
	{
		readonly IEqualityComparer<TKey> defaultKeyComparer;
		readonly IComparer<double> defaultValueComparer;
		TKey[] keys = new TKey[1];
		double[] values = new double[1];
		public int Count { get; private set; }
		public HeapDouble(IEqualityComparer<TKey> equalityKeyComparer) => defaultKeyComparer = equalityKeyComparer;
		public HeapDouble() : this(EqualityComparer<TKey>.Default) { }
		/// <summary>
		///     堆增加一个元素
		/// </summary>
		/// <param name="element"></param>
		/// <param name="sortingValue"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(TKey element, double sortingValue)
		{
			if (Count >= keys.Length)
			{
				var newLength = Count << 1;
				Array.Resize(ref keys, newLength);
				Array.Resize(ref values, newLength);
			}
			keys[Count] = element;
			values[Count] = sortingValue;
			AdjustUp(Count++);
			/*
#if UNITY_EDITOR
			if (Find(element, sortingValue) < 0)
				throw new("Heap Add Error");
#endif
			*/
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TKey Pop(out double value)
		{
			value = values[0];
			return Pop();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TKey Pop()
		{
			var result = keys[0];
			RemoveAt(0);
			return result;
		}
		/// <summary>
		///     移除指定元素。若keyValue不匹配则不移除
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(TKey key, double value)
		{
			var index = Find(key, value);
			if (index > 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryPop(out TKey key, out double value)
		{
			if (0 >= Count)
			{
				key = default;
				value = default;
				return false;
			}
			key = Pop(out value);
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TrimExcess()
		{
			Array.Resize(ref keys, Count);
			Array.Resize(ref values, Count);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TKey Peek(out double value)
		{
			value = values[0];
			return keys[0];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TKey Peek() => keys[0];
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryPeek(out TKey key)
		{
			if (Count > 0)
			{
				key = Peek();
				return true;
			}
			key = default;
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryPeek(out TKey key, out double value)
		{
			if (Count > 0)
			{
				key = Peek(out value);
				return true;
			}
			key = default;
			value = default;
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(TKey element, double sortingValue)
		{
			for (var i = 0; i < Count; i++)
				if (defaultKeyComparer.Equals(keys[i], element))
				{
					Update(i, element, sortingValue);
					return;
				}
			throw new ArgumentException($"Heap Update Error {element} {sortingValue}");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(TKey element, double oldValue, double newValue)
		{
			var index = Find(element, oldValue);
			if (index >= 0)
				Update(index, element, newValue);
			else
				throw new ArgumentException($"Heap Update Error {element} {oldValue} {newValue}");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddOrUpdate(TKey element, double sortingValue, IEqualityComparer<TKey> equalityComparer)
		{
			var index = Find(element, equalityComparer);
			if (index >= 0)
				Update(index, element, sortingValue);
			else
				Add(element, sortingValue);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddOrUpdate(
			TKey element,
			double oldValue,
			double newValue,
			IEqualityComparer<TKey> equalityComparer)
		{
			var index = Find(element, oldValue, equalityComparer);
			if (index >= 0)
				Update(index, element, newValue);
			else
				Add(element, newValue);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddOrUpdate(TKey element, double sortingValue) => AddOrUpdate(element, sortingValue, defaultKeyComparer);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddOrUpdate(TKey element, double oldValue, double newValue) => AddOrUpdate(element, oldValue, newValue, defaultKeyComparer);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			Array.Clear(keys, 0, keys.Length);
			Array.Clear(values, 0, values.Length);
			Count = 0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Find(TKey element, double sortingValue) => Find(element, sortingValue, defaultKeyComparer);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void RemoveAt(int index)
		{
			keys[index] = keys[--Count];
			values[index] = values[Count];
			AdjustDown(Count, index);
			AdjustUp(index);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int Find(TKey element, double sortingValue, IEqualityComparer<TKey> equalityComparer)
		{
			var count = Count;
			if (count <= 0) return -1;
			var depth = Mathf.CeilToInt(Mathf.Log(count, 2));
			var stack = ArrayPool<int>.Shared.Rent(depth);
			var stackCount = 1;
			stack[0] = 0;
			while (stackCount > 0)
			{
				var currentIndex = stack[--stackCount];
				var currentValue = values[currentIndex];
				if (sortingValue < currentValue) continue;
				if (sortingValue == currentValue && equalityComparer.Equals(keys[currentIndex], element))
				{
					ArrayPool<int>.Shared.Return(stack);
					return currentIndex;
				}
				var leftIndex = (currentIndex << 1) | 1;
				if (leftIndex >= count) continue;
				stack[stackCount++] = leftIndex;
				var rightIndex = leftIndex + 1;
				if (rightIndex >= count) continue;
				stack[stackCount++] = rightIndex;
			}
			ArrayPool<int>.Shared.Return(stack);
			return -1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int Find(TKey element, IEqualityComparer<TKey> equalityComparer)
		{
			var count = Count;
			for (var i = 0; i < count; i++)
				if (equalityComparer.Equals(keys[i], element))
					return i;
			return -1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Update(int index, TKey key, double value)
		{
			keys[index] = key;
			values[index] = value;
			AdjustDown(Count, index);
			AdjustUp(index);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AdjustDown(int length, int index)
		{
			var values = this.values;
			var keys = this.keys;
			while (true)
			{
				var leftIndex = (index << 1) | 1;
				if (leftIndex >= length) return;
				var rightIndex = leftIndex + 1;
				var leftValue = values[leftIndex];
				var currentValue = values[index];
				TKey tempKey;
				double tempValue;
				if (rightIndex >= length)
				{
					if (leftValue >= currentValue) return;
					tempKey = keys[leftIndex];
					keys[leftIndex] = keys[index];
					keys[index] = tempKey;
					tempValue = leftValue;
					values[leftIndex] = currentValue;
					values[index] = tempValue;
					return;
				}
				if (leftValue < currentValue)
				{
					if (values[rightIndex] < leftValue)
					{
						tempKey = keys[rightIndex];
						keys[rightIndex] = keys[index];
						keys[index] = tempKey;
						tempValue = values[rightIndex];
						values[rightIndex] = currentValue;
						values[index] = tempValue;
						index = rightIndex;
					}
					else
					{
						tempKey = keys[leftIndex];
						keys[leftIndex] = keys[index];
						keys[index] = tempKey;
						tempValue = leftValue;
						values[leftIndex] = currentValue;
						values[index] = tempValue;
						index = leftIndex;
					}
				}
				else
				{
					if (values[rightIndex] < currentValue)
					{
						tempKey = keys[rightIndex];
						keys[rightIndex] = keys[index];
						keys[index] = tempKey;
						tempValue = values[rightIndex];
						values[rightIndex] = currentValue;
						values[index] = tempValue;
						index = rightIndex;
					}
					else
					{
						return;
					}
				}
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AdjustUp(int index)
		{
			while (index > 0)
			{
				var parentIndex = (index - 1) >> 1;
				var currentValue = values[index];
				var parentValue = values[parentIndex];
				if (currentValue < parentValue)
				{
					var tempKey = keys[parentIndex];
					keys[parentIndex] = keys[index];
					values[parentIndex] = currentValue;
					keys[index] = tempKey;
					values[index] = parentValue;
					index = parentIndex;
				}
				else
				{
					break;
				}
			}
		}
	}
}
