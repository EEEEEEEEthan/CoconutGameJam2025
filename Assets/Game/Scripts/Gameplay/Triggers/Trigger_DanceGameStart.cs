using UnityEngine;
using Game.Gameplay.DanceGame;

namespace Game.Gameplay.Triggers
{
    /// <summary>
    /// 跳舞游戏启动触发器
    /// 当玩家进入触发区域时，启动跳舞游戏
    /// </summary>
    public class Trigger_DanceGameStart : GameTrigger
    {
        [SerializeField] private DanceGameManager danceGameManager;
        
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/触发器/跳舞游戏启动")]
        static void Create()
        {
            var triggerObject = Create(UnityEditor.Selection.activeTransform);
            triggerObject.name = nameof(Trigger_DanceGameStart);
            triggerObject.AddComponent<Trigger_DanceGameStart>();
            UnityEditor.Selection.activeGameObject = triggerObject;
        }
#endif
        
        void OnTriggerEnter(Collider other)
        {
            if (other != GameRoot.Player.PlayerPositionTrigger) return;
            
            if (danceGameManager != null)
            {
                // 调用DanceGameManager的Start方法，传入空的回调
                danceGameManager.Start(null);
            }
            else
            {
                Debug.LogWarning("DanceGameManager reference is missing!", this);
            }
            
            TryTrigger();
        }
    }
}