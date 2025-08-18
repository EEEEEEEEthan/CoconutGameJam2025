using System;
using UnityEngine;
namespace Game.Gameplay
{
	public class UICamera: GameBehaviour
	{
		[SerializeField] Rigidbody[] rigidbodies;
		void Update()
		{
			var calculator = GameRoot.CameraController.VelocityCalculator;
			var acceleration = calculator.Acceleration;
			var velocity = calculator.Velocity;
			foreach (var rigidbody in rigidbodies)
			{
				rigidbody.AddForce(-acceleration);
				rigidbody.AddForce(-velocity);
			}
		}
	}
}
