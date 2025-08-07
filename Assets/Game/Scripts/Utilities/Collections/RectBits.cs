using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Game.Utilities.Collections
{
	public interface IReadOnlyRectBits : IReadOnlyArray2D<bool>
	{
		public IEnumerable<int> GetAllIndices();
		public void GetAllIndices(ICollection<int> collection);
		public IEnumerable<Vector2Int> GetAllPositions();
	}
	public sealed class RectBits : IReadOnlyRectBits
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void GetIndex(int index, out int ulongIndex, out int bitIndex)
		{
			const int mask = (1 << 6) - 1;
			ulongIndex = index >> 6;
			bitIndex = index & mask;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int GetIndex(int ulongIndex, int bitIndex) => (ulongIndex << 6) | bitIndex;
		public readonly RectIndexMapper indexMapper;
		readonly ulong[] bits;
		public bool IsEmpty
		{
			get
			{
				for (var i = bits.Length; i-- > 0;)
					if (bits[i] != 0)
						return false;
				return true;
			}
		}
		RectIndexMapper IReadOnlyArray2D<bool>.IndexMapper => indexMapper;
		public bool this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (!indexMapper.Contains(index))
					throw new ArgumentOutOfRangeException(nameof(index),
						index,
						$"Index out of range(0~{indexMapper.count})");
				return GetValueUnchecked(index);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				if (!indexMapper.Contains(index))
					throw new ArgumentOutOfRangeException(nameof(index),
						index,
						$"Index out of range(0~{indexMapper.count})");
				SetValueUnchecked(index, value);
			}
		}
		public bool this[int x, int y]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (!indexMapper.Contains(x, y))
					throw new ArgumentOutOfRangeException($"{nameof(x)},{nameof(y)}",
						$"{(x, y)}",
						$"Position out of range({indexMapper.rect})");
				return GetValueUnchecked(x, y);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				if (!indexMapper.Contains(x, y))
					throw new ArgumentOutOfRangeException($"{nameof(x)},{nameof(y)}",
						$"{(x, y)}",
						$"Position out of range({indexMapper.rect})");
				SetValueUnchecked(x, y, value);
			}
		}
		public bool this[Vector2Int position]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (!indexMapper.Contains(position))
					throw new ArgumentOutOfRangeException($"{nameof(position)}",
						$"{position}",
						$"Position out of range({indexMapper.rect})");
				return GetValueUnchecked(position);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				if (!indexMapper.Contains(position))
					throw new ArgumentOutOfRangeException($"{nameof(position)}",
						$"{position}",
						$"Position out of range({indexMapper.rect})");
				SetValueUnchecked(position, value);
			}
		}
		public RectBits(RectInt rect)
		{
			indexMapper = new(rect);
			bits = new ulong[Mathf.CeilToInt(indexMapper.count / 64f)];
		}
		public RectBits(RectInt rect, BinaryReader reader) : this(rect) => Deserialize(reader);
		public void Serialize(BinaryWriter writer)
		{
			using (writer.WriteScope(nameof(RectBits).GetHashCode()))
			{
				foreach (var bit in bits) writer.Write(bit);
			}
		}
		public void Deserialize(BinaryReader reader)
		{
			using (reader.ReadScope(nameof(RectBits).GetHashCode()))
			{
				var count = bits.Length;
				for (var i = 0; i < count; ++i) bits[i] = reader.ReadUInt64();
			}
		}
		public IEnumerable<int> GetAllIndices()
		{
			var length = bits.Length;
			for (var ulongIndex = 0; ulongIndex < length; ++ulongIndex)
				if (bits[ulongIndex] != 0)
					for (var bitIndex = 0; bitIndex < 64; ++bitIndex)
						if (Get(ulongIndex, bitIndex))
							yield return GetIndex(ulongIndex, bitIndex);
		}
		public void GetAllIndices(ICollection<int> collection)
		{
			var length = bits.Length;
			for (var ulongIndex = 0; ulongIndex < length; ++ulongIndex)
				if (bits[ulongIndex] != 0)
					for (var bitIndex = 0; bitIndex < 64; ++bitIndex)
						if (Get(ulongIndex, bitIndex))
							collection.Add(GetIndex(ulongIndex, bitIndex));
		}
		public IEnumerable<Vector2Int> GetAllPositions()
		{
			var length = bits.Length;
			for (var ulongIndex = 0; ulongIndex < length; ++ulongIndex)
				if (bits[ulongIndex] != 0)
					for (var bitIndex = 0; bitIndex < 64; ++bitIndex)
						if (Get(ulongIndex, bitIndex))
							yield return indexMapper.GetPosition(GetIndex(ulongIndex, bitIndex));
		}
		public void GetAllPositions(ICollection<Vector2Int> collection, bool reachableOnly)
		{
			var length = bits.Length;
			for (var ulongIndex = 0; ulongIndex < length; ++ulongIndex)
				if (bits[ulongIndex] != 0)
					for (var bitIndex = 0; bitIndex < 64; ++bitIndex)
						if (Get(ulongIndex, bitIndex))
							collection.Add(indexMapper.GetPosition(GetIndex(ulongIndex, bitIndex)));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetValueUnchecked(int index)
		{
			GetIndex(index, out var ulongIndex, out var bitIndex);
			return Get(ulongIndex, bitIndex);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetValueUnchecked(int x, int y) => GetValueUnchecked(indexMapper.GetIndexUnchecked(x, y));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetValueUnchecked(Vector2Int position) => GetValueUnchecked(indexMapper.GetIndexUnchecked(position));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetValueUnchecked(int index, bool value)
		{
			GetIndex(index, out var ulongIndex, out var bitIndex);
			if (value)
				bits[ulongIndex] |= 1ul << bitIndex;
			else
				bits[ulongIndex] &= ~(1ul << bitIndex);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetValueUnchecked(int x, int y, bool value) => SetValueUnchecked(indexMapper.GetIndexUnchecked(x, y), value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetValueUnchecked(Vector2Int position, bool value) => SetValueUnchecked(indexMapper.GetIndexUnchecked(position), value);
		public void SetAll(bool value) => bits.MemSet(value ? ulong.MaxValue : 0ul);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool Get(int ulongIndex, int bitIndex) => (bits[ulongIndex] & (1ul << bitIndex)) != 0;
		bool IReadOnlyArray2D<bool>.TryGetValue(int index, out bool value)
		{
			if (indexMapper.Contains(index))
			{
				value = GetValueUnchecked(index);
				return true;
			}
			value = false;
			return false;
		}
		bool IReadOnlyArray2D<bool>.TryGetValue(int x, int y, out bool value)
		{
			if (indexMapper.Contains(x, y))
			{
				value = GetValueUnchecked(x, y);
				return true;
			}
			value = false;
			return false;
		}
		bool IReadOnlyArray2D<bool>.TryGetValue(Vector2Int position, out bool value)
		{
			if (indexMapper.Contains(position))
			{
				value = GetValueUnchecked(position);
				return true;
			}
			value = false;
			return false;
		}
	}
}
