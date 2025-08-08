using UnityEngine;
namespace Game.Gameplay
{
	public class GameBehaviour : MonoBehaviour
	{
		[SerializeField] GameRoot root;
		public GameRoot Root => root ??= GetComponentInParent<GameRoot>();
	}
}
