using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay
{
	public class Water : GameBehaviour
	{
		[SerializeField] ParticleSystem splashParticle;
		void OnTriggerEnter(Collider other)
		{
			var playerHandIkInput = GameRoot.Player.HandIkInput;
			if (other == playerHandIkInput.LeftCollider) { }
			else if (other == playerHandIkInput.RightCollider) { }
			else
			{
				return;
			}
			Instantiate(splashParticle, other.transform.position.WithZ(0), Quaternion.Euler(-90, 0, 0));
		}
	}
}
