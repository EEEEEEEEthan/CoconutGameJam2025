using UnityEngine;
namespace Game.Gameplay.触发器
{
	public class 行为_阻塞输入 : GameBehaviour
	{
		[SerializeField] InputBlock block;
		void OnEnable() => GameRoot.Player.inputBlock = block;
		void OnDisable() => GameRoot.Player.inputBlock = default;
	}
}
