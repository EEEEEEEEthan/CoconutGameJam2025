using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Utilities.Collections
{
	public sealed class HeapSingle<TKey> : IHeap<TKey, float>
	{
		readonly IEqualityComparer<TKey> defaultKeyComparer;
		readonly IComparer<float> defaultValueComparer;
		TKey[] keys;
		float[] values;
		IDisposable keyRecycler;
		IDisposable valueRecycler;
		public int Count { get; private set; }
		public HeapSingle(IEqualityComparer<TKey> equalityKeyComparer)
		{
			defaultKeyComparer = equalityKeyComparer;
			keyRecycler = ArrayPoolThreaded<TKey>.RentWithoutDefaultValue(1, out keys);
			valueRecycler = ArrayPoolThreaded<float>.RentWithoutDefaultValue(1, out values);
		}
		public HeapSingle() : this(EqualityComparer<TKey>.Default) { }
		/// <summary>
		///     堆增加一个元素
		/// </summary>
		/// <param name="element"></param>
		/// <param name="sortingValue"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(TKey element, float sortingValue)
		{
			if (Count >= keys.Length)
			{
				var newLength = Count << 1;
				var keyRecycler = ArrayPoolThreaded<TKey>.RentWithoutDefaultValue(newLength, out var keyArray);
				var valueRecycler = ArrayPoolThreaded<float>.RentWithoutDefaultValue(newLength, out var valueArray);
				Array.Copy(keys, keyArray, keys.Length);
				Array.Copy(values, valueArray, values.Length);
				this.keyRecycler.Dispose();
				this.valueRecycler.Dispose();
				this.keyRecycler = keyRecycler;
				this.valueRecycler = valueRecycler;
				keys = keyArray;
				values = valueArray;
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
		public TKey Pop(out float value)
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
		public bool Remove(TKey key, float value)
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
		public bool TryPop(out TKey key, out float value)
		{
			if (0 >= Count)
			{
				key = default;
				value = 0;
				return false;
			}
			key = Pop(out value);
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryPop(out TKey key)
		{
			if (0 >= Count)
			{
				key = default;
				return false;
			}
			key = Pop(out _);
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TrimExcess()
		{
			Array.Resize(ref keys, Count);
			Array.Resize(ref values, Count);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TKey Peek(out float value)
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
		public bool TryPeek(out TKey key, out float value)
		{
			if (Count > 0)
			{
				key = Peek(out value);
				return true;
			}
			key = default;
			value = 0;
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(TKey element, float sortingValue)
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
		public void Update(TKey element, float oldValue, float newValue)
		{
			var index = Find(element, oldValue);
			if (index < 0)
				Debug.LogError($"Heap Update Error {element} {oldValue} {newValue}");
			else
				Update(index, element, newValue);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddOrUpdate(TKey element, float sortingValue, IEqualityComparer<TKey> equalityComparer)
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
			float oldValue,
			float newValue,
			IEqualityComparer<TKey> equalityComparer)
		{
			var index = Find(element, oldValue, equalityComparer);
			if (index >= 0)
				Update(index, element, newValue);
			else
				Add(element, newValue);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddOrUpdate(TKey element, float sortingValue) => AddOrUpdate(element, sortingValue, defaultKeyComparer);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddOrUpdate(TKey element, float oldValue, float newValue) => AddOrUpdate(element, oldValue, newValue, defaultKeyComparer);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			Array.Clear(keys, 0, keys.Length);
			Array.Clear(values, 0, values.Length);
			Count = 0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Find(TKey element, float sortingValue) => Find(element, sortingValue, defaultKeyComparer);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index)
		{
			keys[index] = keys[--Count];
			values[index] = values[Count];
			AdjustDown(Count, index);
			AdjustUp(index);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int Find(TKey element, float sortingValue, IEqualityComparer<TKey> equalityComparer)
		{
			var count = Count;
			if (count <= 0) return -1;
			// 先检查堆顶
			var topValue = values[0];
			if (sortingValue < topValue) return -1;
			var depth = Mathf.CeilToInt(Mathf.Log(count, 2));
			using (ArrayPoolThreaded<int>.RentWithoutDefaultValue(depth, out var stack))
			{
				var stackCount = 1;
				stack[0] = 0;
				while (stackCount > 0)
				{
					var currentIndex = stack[--stackCount];
					var currentValue = values[currentIndex];
					if (sortingValue < currentValue) continue;
					if (sortingValue == currentValue && equalityComparer.Equals(keys[currentIndex], element)) return currentIndex;
					var leftIndex = (currentIndex << 1) | 1;
					if (leftIndex < count)
					{
						stack[stackCount++] = leftIndex;
						var rightIndex = leftIndex + 1;
						if (rightIndex < count) stack[stackCount++] = rightIndex;
					}
				}
			}
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
		void Update(int index, TKey key, float value)
		{
			keys[index] = key;
			values[index] = value;
			AdjustDown(Count, index);
			AdjustUp(index);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AdjustDown(int length, int index)
		{
			var keys = this.keys;
			var values = this.values;
			while (true)
			{
				var leftIndex = (index << 1) | 1;
				if (leftIndex >= length) return;
				var rightIndex = leftIndex + 1;
				var leftValue = values[leftIndex];
				var currentValue = values[index];
				TKey tempKey;
				float tempValue;
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
