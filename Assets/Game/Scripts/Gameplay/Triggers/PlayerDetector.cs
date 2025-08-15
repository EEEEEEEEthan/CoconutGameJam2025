using UnityEngine;
namespace Game.Gameplay.Triggers
{
	public sealed class PlayerDetector : GameBehaviour
	{
		public bool PlayerInside { get; private set; }
		void Awake()
		{
			if (TryGetComponent<MeshRenderer>(out var renderer)) renderer.enabled = false;
		}
		void OnTriggerEnter(Collider other)
		{
			if (other != GameRoot.Player.PlayerPositionTrigger) return;
			PlayerInside = true;
		}
		void OnTriggerExit(Collider other)
		{
			if (other != GameRoot.Player.PlayerPositionTrigger) return;
			PlayerInside = false;
		}
	}
}
