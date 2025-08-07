using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Utilities.UnityTools
{
	public static class GizmosEx
	{
		public static void DrawArrow(
			Vector3 from,
			Vector3 to,
			float arrowHeadLength = 0.25f,
			float arrowHeadAngle = 20.0f)
		{
			Gizmos.DrawLine(from, to);
			var direction = to - from;
			var right = Quaternion.LookRotation(direction) *
				Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
				Vector3.forward;
			var left = Quaternion.LookRotation(direction) *
				Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
				Vector3.forward;
			Gizmos.DrawRay(to, right * arrowHeadLength);
			Gizmos.DrawRay(to, left * arrowHeadLength);
		}
		public static void DrawPath(params Vector3[] points)
		{
			var length = points.Length;
			for (var i = 1; i < length; i++) Gizmos.DrawLine(points[i], points[i - 1]);
		}
		public static void DrawRect(RectTransform rectTransform)
		{
			using var _ = ArrayPoolThreaded<Vector3>.RentWithoutDefaultValue(4, out var points);
			rectTransform.GetWorldCorners(points);
			var length = points.Length;
			for (var i = 1; i < length; i++) Gizmos.DrawLine(points[i], points[i - 1]);
			Gizmos.DrawLine(points[0], points[length - 1]);
		}
	}
}
