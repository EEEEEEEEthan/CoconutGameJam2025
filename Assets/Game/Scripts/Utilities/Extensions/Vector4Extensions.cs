using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static Vector4 WithW(this Vector4 @this, float w) => new(@this.x, @this.y, @this.z, w);
		public static Vector4 WithX(this Vector4 @this, float x) => new(x, @this.y, @this.z, @this.w);
		public static Vector4 WithXW(this Vector4 @this, float x, float w) => new(x, @this.y, @this.z, w);
		public static Vector4 WithXY(this Vector4 @this, float x, float y) => new(x, y, @this.z, @this.w);
		public static Vector4 WithXYW(this Vector4 @this, float x, float y, float w) => new(x, y, @this.z, w);
		public static Vector4 WithXYZ(this Vector4 @this, float x, float y, float z) => new(x, y, z, @this.w);
		public static Vector4 WithXZ(this Vector4 @this, float x, float z) => new(x, @this.y, z, @this.w);
		public static Vector4 WithXZW(this Vector4 @this, float x, float z, float w) => new(x, @this.y, z, w);
		public static Vector4 WithY(this Vector4 @this, float y) => new(@this.x, y, @this.z, @this.w);
		public static Vector4 WithYW(this Vector4 @this, float y, float w) => new(@this.x, y, @this.z, w);
		public static Vector4 WithYZ(this Vector4 @this, float y, float z) => new(@this.x, y, z, @this.w);
		public static Vector4 WithYZW(this Vector4 @this, float y, float z, float w) => new(@this.x, y, z, w);
		public static Vector4 WithZ(this Vector4 @this, float z) => new(@this.x, @this.y, z, @this.w);
		public static Vector4 WithZW(this Vector4 @this, float z, float w) => new(@this.x, @this.y, z, w);
	}
}
