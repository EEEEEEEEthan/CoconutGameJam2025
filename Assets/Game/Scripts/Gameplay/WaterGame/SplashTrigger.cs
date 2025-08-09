using Game.ResourceManagement;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class SplashTrigger : GameBehaviour
	{
		void OnTriggerEnter(Collider other)
		{
			var position = other.transform.position.WithY(transform.position.y);
			Instantiate(ResourceTable.splashPrefab.Main, position, Quaternion.Euler(-90, 0, 0));
		}
		void OnTriggerExit(Collider other)
		{
			var position = other.transform.position.WithY(transform.position.y);
			Instantiate(ResourceTable.splashPrefab.Main, position, Quaternion.Euler(-90, 0, 0));
		}
	}
}
