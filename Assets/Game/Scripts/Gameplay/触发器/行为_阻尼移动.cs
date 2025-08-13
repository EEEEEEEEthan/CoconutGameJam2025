using UnityEngine;
namespace Game.Gameplay.触发器
{
	public class 行为_阻尼移动 : GameBehaviour
	{
		[SerializeField] Transform target;
		[SerializeField] float smoothTime = 0.3f;
		[SerializeField] float maxSpeed = float.MaxValue;
		Vector3 velocity;
		void Update()
		{
			if (!target) return;
			transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, smoothTime, maxSpeed, Time.deltaTime);
			transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime / smoothTime);
		}
		void OnEnable() => velocity = Vector3.zero;
		void OnDisable() => velocity = Vector3.zero;
		void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;
			if (target) Gizmos.DrawLine(transform.position, target.position);
		}
	}
}
