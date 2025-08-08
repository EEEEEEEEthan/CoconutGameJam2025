using Game.Utilities;
using ReferenceHelper;
using UnityEngine;
using UnityEngine.Animations.Rigging;
namespace Game.Gameplay
{
	public class Leg : MonoBehaviour
	{
		[SerializeField, ObjectReference,] TwoBoneIKConstraint constraint;
		[SerializeField] Transform stand;
		[SerializeField] Transform raiseForward;
		[SerializeField] float smoothTime = 0.3f;
		[SerializeField] float minDistance;
		[SerializeField] Vector2 xAngleRange;
		Vector3 positionVelocity;
		Vector3 rotationVelocity;
		void Update()
		{
			var root = constraint.data.root;
			if (!root) return;
			var target = constraint.data.target;
			if (!target) return;
			var mid = constraint.data.mid;
			if (!mid) return;
			var tip = constraint.data.tip;
			if (!tip) return;
			// 检测右箭头键输入
			var targetTransform = Input.GetKey(KeyCode.RightArrow) ? raiseForward : stand;
			// 平滑移动和旋转到Forward位置
			var position = Vector3.SmoothDamp(target.position, targetTransform.position, ref positionVelocity, smoothTime);
			var direction = position - root.position;
			if (direction.magnitude < minDistance) position = root.position + direction.normalized * minDistance;
			target.position = position;
			var rotation = Quaternion.Slerp(target.rotation, targetTransform.rotation, Time.deltaTime / smoothTime);
		}
		void OnDrawGizmos()
		{
			var root = constraint.data.root;
			if (root) Gizmos.DrawSphere(root.position, minDistance);
		}
	}
}
