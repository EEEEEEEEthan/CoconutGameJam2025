using Game.Gameplay.WaterGame;
using UnityEngine;
namespace Game.Gameplay.Hints
{
	public class Hint : GameBehaviour
	{
		[SerializeField] Rigidbody stuff;
		[SerializeField] float velocityMultiplier = 1f;
		[SerializeField] float accelerationMultiplier = 1f;
		VelocityCalculator cameraControllerVelocityCalculator;
		void Update()
		{
			cameraControllerVelocityCalculator = GameRoot.CameraController.VelocityCalculator;
			var force = cameraControllerVelocityCalculator.Velocity * velocityMultiplier;
			force += cameraControllerVelocityCalculator.Acceleration * accelerationMultiplier;
			if (force.x != 0 || force.y != 0 || force.z != 0)
				if (force.x != float.NaN && force.y != float.NaN && force.z != float.NaN)
					if (!float.IsInfinity(force.x) && !float.IsInfinity(force.y) && !float.IsInfinity(force.z))
						stuff.AddForce(force);
		}
	}
}
