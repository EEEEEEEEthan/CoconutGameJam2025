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
		bool visible;
		void Awake() => hook.transform.localPosition = new(0, 5, 0);
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
				var endTime = Time.time + 0.5f;
				while (Time.time < endTime)
				{
					var t = (Time.time - startTime) / (endTime - startTime);
					var y = showCurve.Evaluate(t);
					y = y.Remapped(0, 1, 5, 0);
					hook.transform.localPosition = new(0, y, 0);
					yield return null;
				}
				hook.transform.localPosition = new(0, 0, 0);
			}
		}
		public void Hide()
		{
			StartCoroutine(hide());
			IEnumerator hide()
			{
				var velocity = Vector3.zero;
				while (true)
				{
					hook.transform.localPosition = Vector3.SmoothDamp(hook.transform.localPosition, new(0, 10, 0), ref velocity, 1f);
					if (hook.localPosition.y > 5) break;
					yield return null;
				}
				gameObject.SetActive(false);
			}
		}
	}
}
