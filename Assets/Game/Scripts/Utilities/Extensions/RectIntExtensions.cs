using System;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static IEnumerable<Vector2Int> IterEdgePositions(this RectInt @this)
		{
			var xMin = @this.xMin;
			var yMin = @this.yMin;
			var xMax = @this.xMax - 1;
			var yMax = @this.yMax - 1;
			for (var x = xMin; x <= xMax; x++)
			{
				yield return Vector2IntExtensions.Create(x, yMin);
				yield return Vector2IntExtensions.Create(x, yMax);
			}
			for (var y = yMin + 1; y < yMax; y++)
			{
				yield return Vector2IntExtensions.Create(xMin, y);
				yield return Vector2IntExtensions.Create(xMax, y);
			}
		}
		public static IEnumerable<Vector2Int> IterNeighborPositions(this RectInt @this, bool withDiagonal = false)
		{
			var xMin = @this.xMin;
			var yMin = @this.yMin;
			var xMax = @this.xMax;
			var yMax = @this.yMax;
			var yNeighborMin = yMin - 1;
			var xNeighborMin = xMin - 1;
			for (var x = xMin; x < xMax; x++)
			{
				yield return Vector2IntExtensions.Create(x, yNeighborMin);
				yield return Vector2IntExtensions.Create(x, yMax);
			}
			for (var y = yMin; y < yMax; y++)
			{
				yield return Vector2IntExtensions.Create(xNeighborMin, y);
				yield return Vector2IntExtensions.Create(xMax, y);
			}
			if (withDiagonal)
			{
				yield return Vector2IntExtensions.Create(xNeighborMin, yNeighborMin);
				yield return Vector2IntExtensions.Create(xNeighborMin, yMax);
				yield return Vector2IntExtensions.Create(xMax, yNeighborMin);
				yield return Vector2IntExtensions.Create(xMax, yMax);
			}
		}
		public static bool IsEdge(this RectInt @this, Vector2Int position) =>
			position.x == @this.xMin ||
			position.x == @this.xMax - 1 ||
			position.y == @this.yMin ||
			position.y == @this.yMax - 1;
		public static bool Contains(this RectInt @this, RectInt other) =>
			@this.xMin <= other.xMin &&
			@this.xMax >= other.xMax &&
			@this.yMin <= other.yMin &&
			@this.yMax >= other.yMax;
		public static Rect ToRect(this RectInt @this) => new(@this.x, @this.y, @this.width, @this.height);
		public static bool TryRandomPick(this RectInt @this, out Vector2Int position)
		{
			if (@this.width <= 0 || @this.height <= 0)
			{
				position = default;
				return false;
			}
			var x = random.Next(@this.xMin, @this.xMax);
			var y = random.Next(@this.yMin, @this.yMax);
			position = Vector2IntExtensions.Create(x, y);
			return true;
		}
		public static Vector2Int RandomPick(this RectInt @this)
		{
			if (@this.TryRandomPick(out var position)) return position;
			throw new ArgumentOutOfRangeException($"wrong size: {@this}");
		}
	}
}
