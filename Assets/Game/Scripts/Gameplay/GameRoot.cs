using UnityEngine;
namespace Game.Gameplay
{
	public class GameRoot : MonoBehaviour { }
	public class GameBehaviour : MonoBehaviour
	{
		[SerializeField] Player player;
		GameRoot gameRoot;
		public Player Player => player;
		public GameRoot GameRoot => gameRoot ??= GetComponentInParent<GameRoot>();
	}
}
