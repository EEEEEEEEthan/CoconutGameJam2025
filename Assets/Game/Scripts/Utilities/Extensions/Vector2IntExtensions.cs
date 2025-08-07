using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Utilities
{
	public static class Vector2IntExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2Int Create(int x, int y)
		{
			var result = Vector2Int.zero;
			result.x = x;
			result.y = y;
			return result;
		}
		public static Vector3Int XY_(this Vector2Int @this, int z)
		{
			var result = Vector3Int.zero;
			result.x = @this.x;
			result.y = @this.y;
			result.z = z;
			return result;
		}
		public static Vector3Int YX_(this Vector2Int @this, int z)
		{
			var result = Vector3Int.zero;
			result.x = @this.y;
			result.y = @this.x;
			result.z = z;
			return result;
		}
		public static Vector3Int X_Y(this Vector2Int @this, int z)
		{
			var result = Vector3Int.zero;
			result.x = @this.x;
			result.y = z;
			result.z = @this.y;
			return result;
		}
		public static Vector3Int Y_X(this Vector2Int @this, int z)
		{
			var result = Vector3Int.zero;
			result.x = @this.y;
			result.y = z;
			result.z = @this.x;
			return result;
		}
		public static Vector3Int _XY(this Vector2Int @this, int z)
		{
			var result = Vector3Int.zero;
			result.x = z;
			result.y = @this.x;
			result.z = @this.y;
			return result;
		}
		public static Vector3Int _YX(this Vector2Int @this, int z)
		{
			var result = Vector3Int.zero;
			result.x = z;
			result.y = @this.y;
			result.z = @this.x;
			return result;
		}
		public static Vector3 XY_(this Vector2Int @this, float z)
		{
			var result = Vector3.zero;
			result.x = @this.x;
			result.y = @this.y;
			result.z = z;
			return result;
		}
		public static Vector3 YX_(this Vector2Int @this, float z)
		{
			var result = Vector3.zero;
			result.x = @this.y;
			result.y = @this.x;
			result.z = z;
			return result;
		}
		public static Vector3 X_Y(this Vector2Int @this, float z)
		{
			var result = Vector3.zero;
			result.x = @this.x;
			result.y = z;
			result.z = @this.y;
			return result;
		}
		public static Vector3 Y_X(this Vector2Int @this, float z)
		{
			var result = Vector3.zero;
			result.x = @this.y;
			result.y = z;
			result.z = @this.x;
			return result;
		}
		public static Vector3 _XY(this Vector2Int @this, float z)
		{
			var result = Vector3.zero;
			result.x = z;
			result.y = @this.x;
			result.z = @this.y;
			return result;
		}
		public static Vector3 _YX(this Vector2Int @this, float z)
		{
			var result = Vector3.zero;
			result.x = z;
			result.y = @this.y;
			result.z = @this.x;
			return result;
		}
		public static int ManhattanDistanceAlong(this Vector2Int @this, Vector2Int other) => Mathf.Abs(@this.x - other.x) + Mathf.Abs(@this.y - other.y);
		public static int ManhattanDistance(this Vector2Int @this) => Mathf.Abs(@this.x) + Mathf.Abs(@this.y);
		public static float EuclideanDistance(this Vector2Int @this, Vector2Int other) => Vector2.Distance(@this, other);
		/// <summary>
		///     从近到远(直线距离)迭代的周围坐标
		/// </summary>
		/// <param name="this"></param>
		/// <param name="maxDistance">最远距离(不含)</param>
		/// <param name="minDistance">最小距离(含)</param>
		/// <returns></returns>
		public static CircleTileSequence.PositionEnumerator IterNeighbors(this Vector2Int @this, float minDistance, float maxDistance) => new(@this, minDistance, maxDistance);
		public static Pooled GetNeighbors(this Vector2Int @this, float minDistance, float maxDistance, out List<Vector2Int> list)
		{
			var disposable = ListPoolThreaded<Vector2Int>.Rent(out list);
			foreach (var item in @this.IterNeighbors(minDistance, maxDistance)) list.Add(item);
			return disposable;
		}
	}
}
