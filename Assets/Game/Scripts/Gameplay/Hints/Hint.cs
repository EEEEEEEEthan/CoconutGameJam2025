using UnityEngine;
namespace Game.Gameplay.Hints
{
	public class Hint : GameBehaviour
	{
		[SerializeField] Rigidbody stuff;
		Vector3 lastPosition;
		Vector3 velocity;
		Vector3 acceleration;
		void Update()
		{
			var currentPosition = transform.position;
			var deltaPosition = currentPosition - lastPosition;
			var currentVelocity = deltaPosition / Time.deltaTime;
			var deltaVelocity = currentVelocity - velocity;
			acceleration = deltaVelocity / Time.deltaTime;
			velocity = currentVelocity;
			stuff.AddForce(-acceleration * 0.1f, ForceMode.Acceleration);
		}
	}
}
