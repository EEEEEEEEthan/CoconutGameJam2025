using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static Vector2Int FloorToInt(this Vector2 @this)
		{
			var result = Vector2Int.zero;
			result.x = Mathf.FloorToInt(@this.x);
			result.y = Mathf.FloorToInt(@this.y);
			return result;
		}
		public static Vector2Int RoundToInt(this Vector2 @this)
		{
			var result = Vector2Int.zero;
			result.x = Mathf.RoundToInt(@this.x);
			result.y = Mathf.RoundToInt(@this.y);
			return result;
		}
		public static Vector3 XY_(this Vector2 @this, float value)
		{
			var result = Vector3.zero;
			result.x = @this.x;
			result.y = @this.y;
			result.z = value;
			return result;
		}
		public static Vector3 YX_(this Vector2 @this, float value)
		{
			var result = Vector3.zero;
			result.x = @this.y;
			result.y = @this.x;
			result.z = value;
			return result;
		}
		public static Vector3 X_Y(this Vector2 @this, float value)
		{
			var result = Vector3.zero;
			result.x = @this.x;
			result.y = value;
			result.z = @this.y;
			return result;
		}
		public static Vector3 Y_X(this Vector2 @this, float value)
		{
			var result = Vector3.zero;
			result.x = @this.y;
			result.y = value;
			result.z = @this.x;
			return result;
		}
		public static Vector3 _XY(this Vector2 @this, float value)
		{
			var result = Vector3.zero;
			result.x = value;
			result.y = @this.x;
			result.z = @this.y;
			return result;
		}
		public static Vector3 _YX(this Vector2 @this, float value)
		{
			var result = Vector3.zero;
			result.x = value;
			result.y = @this.y;
			result.z = @this.x;
			return result;
		}
		public static float ManhattanDistance(this Vector2 @this) => Mathf.Abs(@this.x) + Mathf.Abs(@this.y);
		/// <summary>
		///     Rotates the Vector2 clockwise by the specified angle (in degrees).
		/// </summary>
		/// <param name="this">The Vector2 to rotate.</param>
		/// <param name="angleDegrees">The angle to rotate (in degrees).</param>
		/// <returns>The rotated Vector2.</returns>
		public static Vector2 RotateClockwise(this Vector2 @this, float angleDegrees)
		{
			var angleRadians = angleDegrees * Mathf.Deg2Rad;
			var cos = Mathf.Cos(angleRadians);
			var sin = Mathf.Sin(angleRadians);
			var result = Vector2.zero;
			result.x = @this.x * cos + @this.y * sin;
			result.y = -@this.x * sin + @this.y * cos;
			return result;
		}
	}
}
