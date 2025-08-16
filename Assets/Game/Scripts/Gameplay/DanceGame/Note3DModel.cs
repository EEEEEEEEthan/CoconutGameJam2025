using System;
using UnityEngine;

namespace Game.Gameplay.DanceGame
{
    /// <summary>
    /// 3D音符对象组件
    /// 负责移动控制、视觉表现和状态通知
    /// </summary>
    public class Note3DModel : MonoBehaviour
    {
        public NoteData noteData;
        
        /// <summary>
        /// 到达目标点时的事件回调
        /// </summary>
        public System.Action<Note3DModel> OnReachTarget;
        
        /// <summary>
        /// 初始化音符数据
        /// </summary>
        /// <param name="data">音符数据</param>
        public void Initialize(NoteData data)
        {
            // TODO: 实现音符初始化逻辑
        }
    }
}