using Game.ResourceManagement;
using Game.Utilities;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class SplashTrigger : GameBehaviour
	{
		void OnCollisionEnter(Collision other)
		{
			var velocity = other.relativeVelocity;
			if (velocity == default)
			{
				if (other.collider.TryGetComponent<VelocityCalculator>(out var calculator))
					velocity = calculator.Velocity;
				else
					return;
			}
			using (ListPoolThreaded<ContactPoint>.Rent(out var contactPoints))
			{
				other.GetContacts(contactPoints);
				var speed = velocity.magnitude;
				var direction = -velocity.normalized;
				foreach (var point in contactPoints)
				{
					var gameObject = Instantiate(ResourceTable.splashPrefab.Main, point.point - direction * 0.01f, Quaternion.LookRotation(direction));
					var particleSystem = gameObject.GetComponent<ParticleSystem>();
					var emission = particleSystem.emission;
					emission.rateOverTime = speed.Remapped(0, 0.5f, 100, 1000).Clamped(100, 1000);
				}
			}
		}
		void OnCollisionExit(Collision other)
		{
			var velocity = other.relativeVelocity;
			if (velocity == default)
			{
				if (other.collider.TryGetComponent<VelocityCalculator>(out var calculator))
					velocity = calculator.Velocity;
				else
					return;
			}
			using (ListPoolThreaded<ContactPoint>.Rent(out var contactPoints))
			{
				other.GetContacts(contactPoints);
				var speed = velocity.magnitude;
				var direction = velocity.normalized;
				foreach (var point in contactPoints)
				{
					var gameObject = Instantiate(ResourceTable.splashPrefab.Main, point.point - direction * 0.01f, Quaternion.LookRotation(direction));
					var particleSystem = gameObject.GetComponent<ParticleSystem>();
					var emission = particleSystem.emission;
					emission.rateOverTime = speed.Remapped(0, 0.5f, 100, 1000).Clamped(100, 1000);
				}
			}
		}
	}
}
