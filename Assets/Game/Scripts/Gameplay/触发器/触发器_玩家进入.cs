using UnityEngine;
namespace Game.Gameplay.触发器
{
	[RequireComponent(typeof(BoxCollider))]
	public class 触发器_玩家进入 : 触发器
	{
		public void OnTriggerEnter(Collider other)
		{
			if (other.GetComponentInParent<Player>()) Trigger();
		}
	}
}
