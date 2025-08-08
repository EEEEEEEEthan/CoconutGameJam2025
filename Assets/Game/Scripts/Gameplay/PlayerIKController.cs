using UnityEngine;
namespace Game.Gameplay
{
	public class PlayerIKController : GameBehaviour
	{
		[SerializeField] Transform leftFoot;
		[SerializeField] Transform rightFoot;
		[SerializeField] Transform playerRoot;
		void Update()
		{
			//playerRoot.position = (leftFoot.position + rightFoot.position)
		}
	}
}
