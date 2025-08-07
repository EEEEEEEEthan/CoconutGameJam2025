using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static Vector3Int FloorToInt(this Vector3 @this) => new(Mathf.FloorToInt(@this.x), Mathf.FloorToInt(@this.y), Mathf.FloorToInt(@this.z));
		public static Vector3Int RoundToInt(this Vector3 @this) => new(Mathf.RoundToInt(@this.x), Mathf.RoundToInt(@this.y), Mathf.RoundToInt(@this.z));
		public static Vector3 WithX(this Vector3 @this) => new(0, @this.y, @this.z);
		public static Vector3 WithY(this Vector3 @this, float y) => new(@this.x, y, @this.z);
		public static Vector3 WithZ(this Vector3 @this, float z) => new(@this.x, @this.y, z);
		public static Vector2 XY(this Vector3 @this) => new(@this.x, @this.y);
		public static Vector2 XZ(this Vector3 @this) => new(@this.x, @this.z);
		public static Vector3 XZY(this Vector3 @this) => new(@this.x, @this.z, @this.y);
		// ReSharper disable once InconsistentNaming
		public static Vector2 YX(this Vector3 @this) => new(@this.y, @this.x);
		public static Vector3 YXZ(this Vector3 @this) => new(@this.y, @this.x, @this.z);
		public static Vector3 YZX(this Vector3 @this) => new(@this.y, @this.z, @this.x);
		public static Vector3 ZXY(this Vector3 @this) => new(@this.z, @this.x, @this.y);
		public static Vector3 ZYX(this Vector3 @this) => new(@this.z, @this.y, @this.x);
	}
}
