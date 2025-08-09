using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class VelocityCalculator : MonoBehaviour
	{
		Rigidbody rigidBody;
		Vector3 lastPosition;
		public Vector3 Velocity { get; private set; }
		void Update()
		{
			Velocity = (transform.position - lastPosition) / Time.deltaTime;
			lastPosition = transform.position;
		}
	}
}
