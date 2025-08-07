using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Game.Utilities
{
	// ReSharper disable once StructCanBeMadeReadOnly
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public struct RectIndexMapper
	{
		struct Enumerator : IEnumerable<Vector2Int>, IEnumerator<Vector2Int>
		{
			readonly RectIndexMapper mapper;
			int currentIndex;
			readonly Vector2Int IEnumerator<Vector2Int>.Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => mapper.GetPositionUnchecked(currentIndex);
			}
			readonly object IEnumerator.Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => mapper.GetPositionUnchecked(currentIndex);
			}
			public Enumerator(RectIndexMapper mapper)
			{
				this.mapper = mapper;
				currentIndex = -1;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			readonly IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator() => this;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			readonly IEnumerator IEnumerable.GetEnumerator() => this;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			bool IEnumerator.MoveNext()
			{
				++currentIndex;
				return currentIndex < mapper.count;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void IEnumerator.Reset() => currentIndex = -1;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void IDisposable.Dispose() { }
		}
		public readonly int widthPower;
		public readonly int heightPower;
		/// <summary>
		///     总宽度(2的整数次方)
		/// </summary>
		public readonly int width;
		/// <summary>
		///     总高度(2的整数次方)
		/// </summary>
		public readonly int height;
		public readonly int xMin;
		public readonly int yMin;
		public readonly int xMax;
		public readonly int yMax;
		/// <summary>
		///     总数量<see cref="width" /> x <see cref="height" />
		/// </summary>
		public readonly int count;
		public readonly RectInt rect;
		readonly int widthMask;
		public IEnumerable<Vector2Int> AllPositionWithin => new Enumerator(this);
		public RectIndexMapper(RectInt rect)
		{
			widthPower = Mathf.CeilToInt(Mathf.Log(rect.width, 2));
			heightPower = Mathf.CeilToInt(Mathf.Log(rect.height, 2));
			width = 1 << widthPower;
			height = 1 << heightPower;
			xMin = rect.xMin;
			yMin = rect.yMin;
			xMax = rect.xMax;
			yMax = rect.yMax;
			widthMask = width - 1;
			count = width * height;
			this.rect = rect;
		}
		public RectIndexMapper(int widthPower) : this(new RectInt(0, 0, 1 << widthPower, 1 << widthPower)) { }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool Contains(int index) => index >= 0 && index < count;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool Contains(int x, int y) => x >= xMin && x < xMax && y >= yMin && y < yMax;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool Contains(Vector2Int position) => Contains(position.x, position.y);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int GetIndexUnchecked(int x, int y) => ((y - yMin) << widthPower) | (x - xMin);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int GetIndexUnchecked(Vector2Int position) => GetIndexUnchecked(position.x, position.y);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int GetIndex(int x, int y)
		{
			if (!Contains(x, y)) throw new ArgumentOutOfRangeException($"({x}, {y}) is not in the rect {rect}");
			return GetIndexUnchecked(x, y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int GetIndex(Vector2Int position) => GetIndex(position.x, position.y);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool TryGetIndex(int x, int y, out int index)
		{
			if (Contains(x, y))
			{
				index = GetIndexUnchecked(x, y);
				return true;
			}
			index = 0;
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool TryGetIndex(Vector2Int position, out int index) => TryGetIndex(position.x, position.y, out index);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int GetXUnchecked(int index) => (index & widthMask) + xMin;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly int GetYUnchecked(int index) => (index >> widthPower) + yMin;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void GetPositionUnchecked(int index, out int x, out int y)
		{
			x = GetXUnchecked(index);
			y = GetYUnchecked(index);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Vector2Int GetPositionUnchecked(int index)
		{
			var result = Vector2Int.zero;
			result.x = GetXUnchecked(index);
			result.y = GetYUnchecked(index);
			return result;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void GetPosition(int index, out int x, out int y)
		{
			if (!Contains(index)) throw new ArgumentOutOfRangeException($"({index}) is not in the rect.");
			GetPositionUnchecked(index, out x, out y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Vector2Int GetPosition(int index)
		{
			GetPosition(index, out var x, out var y);
			var result = Vector2Int.zero;
			result.x = x;
			result.y = y;
			return result;
		}
	}
}
