using Game.ResourceManagement;
using Game.Utilities;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class SplashTrigger : GameBehaviour
	{
		[SerializeField] GameObject overrideSplashPrefab;
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
				var rotation = overrideSplashPrefab ? Quaternion.identity : Quaternion.LookRotation(direction);
				var prefab = overrideSplashPrefab ? overrideSplashPrefab : ResourceTable.splashPrefab.Main;
				if (contactPoints.Count <= 0)
				{
					var point = other.collider.ClosestPoint(other.transform.position.WithY(transform.position.y));
					var position = overrideSplashPrefab ? point + Vector3.down * 0.02f : point - direction * 0.01f;
					var gameObject = Instantiate(prefab, position, rotation);
					var particleSystem = gameObject.GetComponent<ParticleSystem>();
					var emission = particleSystem.emission;
					emission.rateOverTime = speed.Remapped(0, 0.5f, 100, 1000).Clamped(100, 1000);
					return;
				}
				foreach (var point in contactPoints)
				{
					var position = overrideSplashPrefab ? point.point + Vector3.down * 0.02f : point.point - direction * 0.01f;
					var gameObject = Instantiate(prefab, position, rotation);
					var particleSystem = gameObject.GetComponent<ParticleSystem>();
					var emission = particleSystem.emission;
					emission.rateOverTime = speed.Remapped(0, 0.5f, 100, 1000).Clamped(100, 1000);
				}
			}
		}
	}
}
