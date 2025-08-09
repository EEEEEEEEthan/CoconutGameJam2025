using UnityEngine;
namespace Game.Gameplay
{
	public class GameRoot : MonoBehaviour
	{
		Player player;
		public Player Player => player ??= GetComponentInChildren<Player>();
	}
	public class GameBehaviour : MonoBehaviour
	{
		GameRoot gameRoot;
		
		public GameRoot GameRoot => gameRoot ??= GetComponentInParent<GameRoot>();
		
	}
}
