using System;
using UnityEngine;
namespace Game.Gameplay.Triggers
{
	public class Trigger_PlayerEmotion : GameTrigger
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("GameObject/触发器/玩家动作")]
		static void Create()
		{
			var triggerObject = Create(UnityEditor.Selection.activeTransform);
			triggerObject.name = nameof(Trigger_PlayerEmotion);
			triggerObject.AddComponent<Trigger_PlayerEmotion>();
			UnityEditor.Selection.activeGameObject = triggerObject;
		}
#endif
		[SerializeField] Player.EmotionCode emotion;
		Action<Player.EmotionCode> onEmotionTriggered;
		protected override void Awake()
		{
			base.Awake();
			onEmotionTriggered = OnEmotionTriggered;
		}
		void OnDisable()
		{
			if (!GameRoot) return;
			GameRoot.Player.OnEmotionTriggered -= onEmotionTriggered;
		}
		void OnTriggerEnter(Collider other)
		{
			if (other != GameRoot.Player.PlayerPositionTrigger) return;
			GameRoot.Player.OnEmotionTriggered += onEmotionTriggered;
		}
		void OnTriggerExit(Collider other)
		{
			if (other != GameRoot.Player.PlayerPositionTrigger) return;
			GameRoot.Player.OnEmotionTriggered -= onEmotionTriggered;
		}
		void OnEmotionTriggered(Player.EmotionCode emotion)
		{
			if (emotion != this.emotion) return;
			TryTrigger();
		}
	}
}
