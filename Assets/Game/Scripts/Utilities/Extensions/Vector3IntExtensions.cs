using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static Vector2Int XY(this Vector3Int @this) => Vector2IntExtensions.Create(@this.x, @this.y);
		public static Vector2Int XZ(this Vector3Int @this) => Vector2IntExtensions.Create(@this.x, @this.z);
		public static Vector3Int XZY(this Vector3Int @this) => new(@this.x, @this.z, @this.y);

		// ReSharper disable once InconsistentNaming
		public static Vector2Int YX(this Vector3Int @this) => Vector2IntExtensions.Create(@this.y, @this.x);
		public static Vector3Int YXZ(this Vector3Int @this) => new(@this.y, @this.x, @this.z);
		public static Vector3Int YZX(this Vector3Int @this) => new(@this.y, @this.z, @this.x);
		public static Vector3Int ZXY(this Vector3Int @this) => new(@this.z, @this.x, @this.y);
		public static Vector3Int ZYX(this Vector3Int @this) => new(@this.z, @this.y, @this.x);
	}
}
