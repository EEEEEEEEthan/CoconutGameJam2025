using Game.ResourceManagement;
using Game.Utilities;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class SplashTrigger : GameBehaviour
	{
		void OnCollisionEnter(Collision other) => HandleSplash(other, true);
		void OnCollisionExit(Collision other) => HandleSplash(other, false);
		void HandleSplash(Collision other, bool isEnter)
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
				var direction = isEnter ? Vector3.Reflect(velocity.normalized, Vector3.up) : velocity.normalized;
				var speed = velocity.magnitude;
				if (contactPoints.Count <= 0)
				{
					var point = other.collider.ClosestPoint(other.transform.position.WithY(transform.position.y));
					var gameObject = Instantiate(ResourceTable.splashPrefab.Main, point - direction * 0.01f, Quaternion.LookRotation(direction));
					var particleSystem = gameObject.GetComponent<ParticleSystem>();
					var emission = particleSystem.emission;
					emission.rateOverTime = speed.Remapped(0, 0.5f, 100, 1000).Clamped(100, 1000);
					return;
				}
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
