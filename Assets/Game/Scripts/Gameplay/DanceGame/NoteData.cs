using UnityEngine;

namespace Game.Gameplay.DanceGame
{
    /// <summary>
    /// 音符数据结构
    /// 包含音符的时间和对应按键信息
    /// </summary>
    [System.Serializable]
    public struct NoteData
    {
        /// <summary>
        /// 音符时间（从游戏开始的秒数）
        /// </summary>
        public float time;
        
        /// <summary>
        /// 对应的键盘按键
        /// </summary>
        public KeyCode key;
    }
}