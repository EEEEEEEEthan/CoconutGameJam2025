using UnityEngine;
namespace Game.FingerRigging
{
	[ExecuteAlways]
	class Finger : MonoBehaviour
	{
		[SerializeField] FingerMuscle muscle;
		[SerializeField] Transform target;
		[SerializeField] Transform hint;
		[SerializeField] Transform handRoot;
		public float MaxDistance => muscle.MaxLength;
		public float TipDistance => (muscle.transform.position - target.position).magnitude;
		public float Progress => muscle.Progress;
		public Transform Tip => muscle.transform;
		public Transform Target => target;
		void LateUpdate()
		{
			if (!target) return;
			var targetDistance = target.position - transform.position;
			muscle.Progress = targetDistance.magnitude / muscle.MaxLength;
			muscle.UpdateDirection();
			var right = handRoot.transform.right;
			var lookDirection = Vector3.Cross(targetDistance, right);
			transform.LookAt(transform.position + lookDirection, targetDistance);
		}
		void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(transform.position, hint.position);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, target.position);
		}
	}
}
