using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay
{
	public class RaycastSource : MonoBehaviour
	{
		[SerializeField, ObjectReference("LeftForward"),]
		Transform leftForwardSource;
		[SerializeField, ObjectReference("Left"),]
		Transform leftSource;
		[SerializeField, ObjectReference("LeftBackward"),]
		Transform leftBackwardSource;
		[SerializeField, ObjectReference("RightForward"),]
		Transform rightForwardSource;
		[SerializeField, ObjectReference("Right"),]
		Transform rightSource;
		[SerializeField, ObjectReference("RightBackward"),]
		Transform rightBackwardSource;
		public Vector3? LeftForwardHitPoint => GetHitPoint(leftForwardSource);
		public Vector3? LeftHitPoint => GetHitPoint(leftSource);
		public Vector3? LeftBackwardHitPoint => GetHitPoint(leftBackwardSource);
		public Vector3? RightForwardHitPoint => GetHitPoint(rightForwardSource);
		public Vector3? RightHitPoint => GetHitPoint(rightSource);
		public Vector3? RightBackwardHitPoint => GetHitPoint(rightBackwardSource);
		public Vector3? CenterHitPoint => GetHitPoint(transform);
		void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			if (LeftForwardHitPoint.HasValue)
			{
				Gizmos.DrawLine(leftForwardSource.position, LeftForwardHitPoint.Value);
				Gizmos.DrawSphere(LeftForwardHitPoint.Value, 0.005f);
			}
			if (LeftHitPoint.HasValue)
			{
				Gizmos.DrawLine(leftSource.position, LeftHitPoint.Value);
				Gizmos.DrawSphere(LeftHitPoint.Value, 0.005f);
			}
			if (LeftBackwardHitPoint.HasValue)
			{
				Gizmos.DrawLine(leftBackwardSource.position, LeftBackwardHitPoint.Value);
				Gizmos.DrawSphere(LeftBackwardHitPoint.Value, 0.005f);
			}
			if (RightForwardHitPoint.HasValue)
			{
				Gizmos.DrawLine(rightForwardSource.position, RightForwardHitPoint.Value);
				Gizmos.DrawSphere(RightForwardHitPoint.Value, 0.005f);
			}
			if (RightHitPoint.HasValue)
			{
				Gizmos.DrawLine(rightSource.position, RightHitPoint.Value);
				Gizmos.DrawSphere(RightHitPoint.Value, 0.005f);
			}
			if (RightBackwardHitPoint.HasValue)
			{
				Gizmos.DrawLine(rightBackwardSource.position, RightBackwardHitPoint.Value);
				Gizmos.DrawSphere(RightBackwardHitPoint.Value, 0.005f);
			}
			if (CenterHitPoint.HasValue)
			{
				Gizmos.DrawLine(transform.position, CenterHitPoint.Value);
				Gizmos.DrawSphere(CenterHitPoint.Value, 0.005f);
			}
		}
		Vector3? GetHitPoint(Transform source)
		{
			var ray = new Ray(source.position, Vector3.down);
			if (Physics.Raycast(ray, out var hit)) return hit.point;
			return null;
		}
	}
}
