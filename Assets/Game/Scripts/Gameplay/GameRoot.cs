using UnityEngine;
namespace Game.Gameplay
{
	public class GameRoot : MonoBehaviour
	{
		[SerializeField] Player player;
		public Player Player => player;
	}
	public class GameBehaviour : MonoBehaviour
	{
		GameRoot gameRoot;
		public GameRoot GameRoot => gameRoot ??= GetComponentInParent<GameRoot>();
	}
}
