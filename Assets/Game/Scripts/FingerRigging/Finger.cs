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
		void LateUpdate()
		{
			if (!target) return;
			var targetDistance = target.position - transform.position;
			muscle.Progress = targetDistance.magnitude / muscle.MaxLength;
			var hintDistance = hint.position - transform.position;
			var right = Vector3.Cross(hintDistance, targetDistance);
			var lookDirection = Vector3.Cross(targetDistance, right);
			var lookUp = hintDistance;
			transform.LookAt(transform.position + lookDirection, lookUp);
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
