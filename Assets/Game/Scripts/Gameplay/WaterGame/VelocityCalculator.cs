using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class VelocityCalculator : MonoBehaviour
	{
		Vector3 lastPosition;
		public Vector3 Velocity { get; private set; }
		public Vector3 Acceleration { get; private set; }
		void Update()
		{
			var velocity = (transform.position - lastPosition) / Time.deltaTime;
			lastPosition = transform.position;
			Acceleration = (Velocity - velocity) / Time.deltaTime;
			Velocity = velocity;
		}
	}
}
