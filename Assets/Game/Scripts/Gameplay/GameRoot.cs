using UnityEngine;
namespace Game.Gameplay
{
	public class GameRoot : MonoBehaviour { }
	public class GameBehaviour : MonoBehaviour
	{
		GameRoot gameRoot;
		public GameRoot GameRoot => gameRoot ??= GetComponentInParent<GameRoot>();
	}
}
