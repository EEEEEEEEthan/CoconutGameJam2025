using UnityEngine;
namespace Game.Utilities
{
	public static class Hermite
	{
		public struct HermiteEvaluator1
		{
			public float p0, v0, p1, v1;
			public readonly float Evaluate(float t)
			{
				var t2 = t * t;
				var t3 = t2 * t;
				var a = 2 * t3 - 3 * t2 + 1;
				var b = t3 - 2 * t2 + t;
				var c = -2 * t3 + 3 * t2;
				var d = t3 - t2;
				return a * p0 + b * v0 + c * p1 + d * v1;
			}
		}
		public struct HermiteTimeline1
		{
			public float p0, v0, p1, v1;
			public float t0, t1;
			public readonly void Evaluate(float t, out float p, out float v)
			{
				t = t.Remapped(t0, t1, 0, 1).Clamped(0, 1);
				var t2 = t * t;
				var t3 = t2 * t;
				var a = 2 * t3 - 3 * t2 + 1;
				var b = t3 - 2 * t2 + t;
				var c = -2 * t3 + 3 * t2;
				var d = t3 - t2;
				p = a * p0 + b * v0 + c * p1 + d * v1;
				v = 6 * a * p0 + 2 * b * v0 + 6 * c * p1 + 2 * d * v1;
			}
		}
		public struct HermiteTimeline2
		{
			public Vector2 p0, v0, p1, v1;
			public float t0, t1;
			public readonly void Evaluate(float t, out Vector2 p, out Vector2 v)
			{
				t = t.Remapped(t0, t1, 0, 1).Clamped(0, 1);
				var t2 = t * t;
				var t3 = t2 * t;
				var a = 2 * t3 - 3 * t2 + 1;
				var b = t3 - 2 * t2 + t;
				var c = -2 * t3 + 3 * t2;
				var d = t3 - t2;
				p = a * p0 + b * v0 + c * p1 + d * v1;
				v = 6 * a * p0 + 2 * b * v0 + 6 * c * p1 + 2 * d * v1;
			}
		}
		public static void Evaluate(float p0, float v0, float p1, float v1, float t, out float p, out float v)
		{
			var t2 = t * t;
			var t3 = t2 * t;
			var a = 2 * t3 - 3 * t2 + 1;
			var b = t3 - 2 * t2 + t;
			var c = -2 * t3 + 3 * t2;
			var d = t3 - t2;
			p = a * p0 + b * v0 + c * p1 + d * v1;
			v = 6 * a * p0 + 2 * b * v0 + 6 * c * p1 + 2 * d * v1;
		}
		public static void Evaluate(
			Vector2 p0,
			Vector2 v0,
			Vector2 p1,
			Vector2 v1,
			float t,
			out Vector2 p,
			out Vector2 v)
		{
			var t2 = t * t;
			var t3 = t2 * t;
			var a = 2 * t3 - 3 * t2 + 1;
			var b = t3 - 2 * t2 + t;
			var c = -2 * t3 + 3 * t2;
			var d = t3 - t2;
			p = a * p0 + b * v0 + c * p1 + d * v1;
			v = 6 * a * p0 + 2 * b * v0 + 6 * c * p1 + 2 * d * v1;
		}
		public static void Evaluate(
			Vector3 p0,
			Vector3 v0,
			Vector3 p1,
			Vector3 v1,
			float t,
			out Vector3 p,
			out Vector3 v)
		{
			var t2 = t * t;
			var t3 = t2 * t;
			var a = 2 * t3 - 3 * t2 + 1;
			var b = t3 - 2 * t2 + t;
			var c = -2 * t3 + 3 * t2;
			var d = t3 - t2;
			p = a * p0 + b * v0 + c * p1 + d * v1;
			v = 6 * a * p0 + 2 * b * v0 + 6 * c * p1 + 2 * d * v1;
		}
		public static void Evaluate(
			Vector4 p0,
			Vector4 v0,
			Vector4 p1,
			Vector4 v1,
			float t,
			out Vector4 p,
			out Vector4 v)
		{
			var t2 = t * t;
			var t3 = t2 * t;
			var a = 2 * t3 - 3 * t2 + 1;
			var b = t3 - 2 * t2 + t;
			var c = -2 * t3 + 3 * t2;
			var d = t3 - t2;
			p = a * p0 + b * v0 + c * p1 + d * v1;
			v = 6 * a * p0 + 2 * b * v0 + 6 * c * p1 + 2 * d * v1;
		}
	}
}
