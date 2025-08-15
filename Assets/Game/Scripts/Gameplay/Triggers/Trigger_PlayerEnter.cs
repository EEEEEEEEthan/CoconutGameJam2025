using UnityEngine;
namespace Game.Gameplay.Triggers
{
	public class Trigger_PlayerEnter : GameTrigger
	{
#if UNITY_EDITOR
		[UnityEditor.MenuItem("GameObject/触发器/玩家动作")]
		static void Create()
		{
			var triggerObject = Create(UnityEditor.Selection.activeTransform);
			triggerObject.name = nameof(Trigger_PlayerEnter);
			triggerObject.AddComponent<Trigger_PlayerEnter>();
			UnityEditor.Selection.activeGameObject = triggerObject;
		}
#endif
		void OnTriggerEnter(Collider other)
		{
			if (other != GameRoot.Player.PlayerPositionTrigger) return;
			TryTrigger();
		}
	}
}
