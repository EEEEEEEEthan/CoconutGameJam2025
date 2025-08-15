using UnityEngine;
namespace Game.Gameplay.Triggers
{
	public class Trigger_PlayerLeave : GameTrigger
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("GameObject/触发器/玩家动作")]
		static void Create()
		{
			var triggerObject = Create(UnityEditor.Selection.activeTransform);
			triggerObject.name = nameof(Trigger_PlayerLeave);
			triggerObject.AddComponent<Trigger_PlayerLeave>();
			UnityEditor.Selection.activeGameObject = triggerObject;
		}
#endif
		[SerializeField] Player.EmotionCode emotion;
		void OnTriggerExit(Collider other)
		{
			if (other != GameRoot.Player.PlayerPositionTrigger) return;
			TryTrigger();
		}
	}
}
