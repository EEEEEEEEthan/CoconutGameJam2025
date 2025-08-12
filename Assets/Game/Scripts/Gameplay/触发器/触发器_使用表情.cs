using UnityEngine;
namespace Game.Gameplay.触发器
{
	public class 触发器_使用表情 : 触发器
	{
		[SerializeField] EmotionCode emotion;
		void OnDisable() => GameRoot.Player.OnEmotionTriggered -= OnEmotionTriggered;
		void OnTriggerEnter(Collider other)
		{
			if (other.GetComponentInParent<Player>()) GameRoot.Player.OnEmotionTriggered += OnEmotionTriggered;
		}
		void OnTriggerExit(Collider other)
		{
			if (other.GetComponentInParent<Player>()) GameRoot.Player.OnEmotionTriggered -= OnEmotionTriggered;
		}
		void OnEmotionTriggered(EmotionCode emotion)
		{
			if (emotion == this.emotion) Trigger();
		}
	}
}
