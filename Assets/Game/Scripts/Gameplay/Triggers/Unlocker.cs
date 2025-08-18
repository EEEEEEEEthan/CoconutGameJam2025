using UnityEngine;
namespace Game.Gameplay.Triggers
{
	public class Unlocker : GameBehaviour
	{
		[SerializeField] KeyCode key;
		void OnEnable() => GameRoot.Player.Unlock(key, true);
	}
}
