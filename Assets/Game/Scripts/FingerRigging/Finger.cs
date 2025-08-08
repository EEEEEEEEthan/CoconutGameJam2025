using System;
using UnityEngine;
namespace Game.FingerRigging
{
	[ExecuteAlways]
	public class Finger : MonoBehaviour
	{
		[SerializeField] FingerMuscle muscle;
		[SerializeField] Transform target;
		[SerializeField] Transform hint;
		[SerializeField] Transform handRoot;
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
