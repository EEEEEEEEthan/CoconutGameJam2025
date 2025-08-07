using System;
using System.Collections.Generic;
namespace Game.Utilities.Collections
{
	/// <summary>
	///     提供对松散集合的只读访问接口。
	///     松散集合是一种支持快速添加、删除和随机访问的数据结构，
	///     允许存在空槽位以提高删除和插入性能。
	/// </summary>
	/// <typeparam name="T">集合中元素的类型，必须是引用类型</typeparam>
	public interface IReadOnlyLooseCollection<out T>
	{
		/// <summary>
		///     获取集合中当前最大有效索引。
		/// </summary>
		int MaxIndex { get; }
		/// <summary>
		///     通过索引获取集合中的元素。
		/// </summary>
		/// <param name="index">要获取的元素索引</param>
		/// <returns>指定索引处的元素，如果该位置已被删除则返回default</returns>
		T this[int index] { get; }
	}
	/// <summary>
	///     线程安全的松散集合实现。
	///     提供快速的添加、删除操作，并通过重用已删除元素的索引来优化内存使用。
	///     所有操作都是线程安全的。
	/// </summary>
	/// <typeparam name="T">集合中元素的类型，必须是引用类型</typeparam>
	public class LooseCollectionThreaded<T> : IReadOnlyLooseCollection<T>
	{
		static void GetIndex(int index, out int longIndex, out int bitIndex)
		{
			longIndex = index >> 6;
			bitIndex = index & 0x3F;
		}
		static int GetIndex(int longIndex, int bitIndex) => (longIndex << 6) | bitIndex;
		readonly List<T> data = new();
		readonly object syncLock = new();
		ulong[] freeIndices = new ulong[1];
		/// <inheritdoc cref="IReadOnlyLooseCollection{T}" />
		public int MaxIndex { get; private set; } = -1;
		/// <inheritdoc cref="IReadOnlyLooseCollection{T}" />
		/// <remarks>不保证读取数据的时效性</remarks>
		// ReSharper disable once InconsistentlySynchronizedField
		public T this[int index]
		{
			// ReSharper disable once InconsistentlySynchronizedField
			get => data[index];
			set => Set(index, value);
		}
		/// <summary>
		///     向集合中添加元素，自动分配索引。
		/// </summary>
		/// <param name="item">要添加的元素</param>
		/// <returns>分配给元素的索引。优先使用已删除元素的索引，如果没有可用索引则分配新的索引</returns>
		/// <remarks>
		///     该方法会优先重用已删除元素的索引以优化内存使用。
		///     如果没有可用的空闲索引，则会在集合末尾添加新元素。
		///     该操作是线程安全的。
		/// </remarks>
		public int Set(T item)
		{
			lock (syncLock)
			{
				var length = freeIndices.Length;
				// 寻找空闲索引
				for (var longIndex = 0; longIndex < length; ++longIndex)
				{
					var longValue = freeIndices[longIndex];
					if (longValue == 0) continue;
					for (var bitIndex = 0; bitIndex < 64; ++bitIndex)
					{
						var mask = 1UL << bitIndex;
						if ((longValue & mask) == 0) continue;
						freeIndices[longIndex] = longValue & ~mask;
						var index = GetIndex(longIndex, bitIndex);
						data[index] = item;
						if (index > MaxIndex) MaxIndex = index;
						return index;
					}
				}
				// 没有空闲索引，添加到末尾
				{
					var index = data.Count;
					data.Add(item);
					if (index > MaxIndex) MaxIndex = index;
					return index;
				}
			}
		}
		/// <summary>
		///     在指定索引位置添加元素。
		/// </summary>
		/// <param name="index">要添加到的索引位置</param>
		/// <param name="item">要添加的元素</param>
		/// <exception cref="ArgumentOutOfRangeException">当索引为负数时抛出</exception>
		/// <remarks>
		///     如果指定的索引超出当前集合大小，会自动扩展集合并填充默认值。
		///     如果指定索引位置已有元素，会覆盖该位置的元素。
		///     该方法会自动清理空闲索引标记，确保索引状态正确。
		///     该操作是线程安全的。
		/// </remarks>
		/// <returns>true-overwrite false-insert</returns>
		public bool Set(int index, T item)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");
			lock (syncLock)
			{
				while (data.Count <= index) data.Add(default);
				data[index] = item;
				if (index > MaxIndex) MaxIndex = index;
				// 清理freeIndices
				GetIndex(index, out var longIndex, out var bitIndex);
				if (longIndex < freeIndices.Length)
				{
					var mask = 1UL << bitIndex;
					if ((freeIndices[longIndex] & mask) != 0)
					{
						freeIndices[longIndex] &= ~mask;
						return true;
					}
				}
				return false;
			}
		}
		/// <summary>
		///     移除指定索引位置的元素。
		/// </summary>
		/// <param name="index">要移除元素的索引位置</param>
		/// <returns>如果成功移除元素返回true；如果索引超出集合范围返回false</returns>
		/// <remarks>
		///     移除元素后，该索引位置会被标记为可用，供后续的Add操作重用。
		///     如果移除的是最后一个元素，会自动更新MaxIndex。
		///     被移除的位置会被设置为default值。
		///     该操作是线程安全的。
		/// </remarks>
		public bool RemoveAt(int index)
		{
			lock (syncLock)
			{
				if (index >= data.Count) return false;
				// 将索引标记为可用
				GetIndex(index, out var longIndex, out var bitIndex);
				if (longIndex >= freeIndices.Length) Array.Resize(ref freeIndices, longIndex + 1);
				if ((freeIndices[longIndex] & (1UL << bitIndex)) != 0) return false; // 已经是空闲的
				freeIndices[longIndex] |= 1UL << bitIndex;
				// 清除数据
				data[index] = default;
				// 更新MaxIndex
				if (index == MaxIndex)
				{
					GetIndex(MaxIndex, out longIndex, out bitIndex);
					for (; longIndex >= 0; --longIndex)
					{
						var longValue = freeIndices[longIndex];
						if (longValue == ulong.MaxValue)
						{
							bitIndex = 63;
							continue;
						}
						for (; bitIndex >= 0; --bitIndex)
						{
							if ((longValue & (1UL << bitIndex)) != 0) continue;
							MaxIndex = GetIndex(longIndex, bitIndex);
							return true;
						}
					}
					MaxIndex = -1;
				}
				return true;
			}
		}
		/// <summary>
		///     清空集合中的所有元素。
		/// </summary>
		/// <remarks>
		///     清空操作会：
		///     - 移除所有元素
		///     - 重置空闲索引标记
		///     - 将MaxIndex重置为0
		///     该操作是线程安全的。
		/// </remarks>
		public void Clear()
		{
			lock (syncLock)
			{
				data.Clear();
				freeIndices.MemSet(0ul);
				MaxIndex = -1;
			}
		}
	}
}
