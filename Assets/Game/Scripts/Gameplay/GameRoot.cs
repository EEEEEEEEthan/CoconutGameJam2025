using Game.Gameplay.WaterGame;
using UnityEngine;
namespace Game.Gameplay
{
	public class GameRoot : MonoBehaviour
	{
		[SerializeField] Player player;
		[SerializeField] WaterGameJudge waterGame;
		public Player Player => player;
		public WaterGameJudge WaterGame => waterGame;
	}
	public class GameBehaviour : MonoBehaviour
	{
		GameRoot gameRoot;
		public GameRoot GameRoot => gameRoot ??= GetComponentInParent<GameRoot>();
	}
}
