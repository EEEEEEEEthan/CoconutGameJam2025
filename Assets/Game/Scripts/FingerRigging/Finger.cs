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
			var distance = transform.position - target.position;
			muscle.Progress = distance.magnitude / muscle.MaxLength;
			var direction = muscle.Direction;
			var targetDirection = target.position - transform.position;
			// 旋转自己,使得targetDirection与direction平行
			if (direction.magnitude > 0.001f && targetDirection.magnitude > 0.001f)
			{
				var rotation = Quaternion.FromToRotation(direction.normalized, targetDirection.normalized);
				transform.rotation = rotation * transform.rotation;
			}
		}
	}
}
