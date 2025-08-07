using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static Vector3 Forward(this Quaternion @this) => @this * Vector3.forward;
		public static Vector3 Up(this Quaternion @this) => @this * Vector3.up;
		public static Vector3 Right(this Quaternion @this) => @this * Vector3.right;
		public static Vector3 Left(this Quaternion @this) => @this * Vector3.left;
		public static Vector3 Back(this Quaternion @this) => @this * Vector3.back;
		public static Vector3 Down(this Quaternion @this) => @this * Vector3.down;
	}
}
