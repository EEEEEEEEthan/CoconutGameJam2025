using Game.ResourceManagement;
using UnityEngine;
namespace Game.Gameplay
{
	public class Water : GameBehaviour
	{
		void OnTriggerEnter(Collider other)
		{
			var playerHandIkInput = GameRoot.Player.HandIkInput;
			if (other == playerHandIkInput.LeftCollider) { }
			else if (other == playerHandIkInput.RightCollider) { }
			else
			{
				return;
			}
			Instantiate(ResourceTable.splashPrefab.Main, other.transform.position + new Vector3(0, -0.02f, 0), Quaternion.Euler(-90, 0, 0));
		}
	}
}
