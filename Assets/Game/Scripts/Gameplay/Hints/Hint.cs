using System.Collections;
using Game.Gameplay.WaterGame;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay.Hints
{
	public class Hint : GameBehaviour
	{
		[SerializeField] Transform hook;
		[SerializeField] Rigidbody stuff;
		[SerializeField] float velocityMultiplier = 1f;
		[SerializeField] float accelerationMultiplier = 1f;
		[SerializeField] AnimationCurve showCurve;
		KeyCode key;
		VelocityCalculator cameraControllerVelocityCalculator;
		bool visible = false;
		void Awake()
		{
			hook.transform.localPosition = new(0, 5, 0);
		}
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
		public void Initialize(KeyCode key) => this.key = key;
		public void Show()
		{
			if (visible) return;
			visible = true;
			StartCoroutine(show());
			IEnumerator show()
			{
				var startTime = Time.time;
				var endTime = Time.time + 1;
				while (Time.time < endTime)
				{
					var t = (Time.time - startTime) / (endTime - startTime);
					var y = showCurve.Evaluate(t);
					y = y.Remapped(0, 1, 5, 0);
					hook.transform.localPosition = new(0, y, 0);
					yield return null;
				}
			}
		}
	}
}
