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
        /// <summary>
        /// 音符移动速度常量（每秒移动距离）
        /// </summary>
        public const float MOVE_SPEED = 0.1f;
        
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
            noteData = data;
        }
        
        /// <summary>
        /// 每帧更新音符位置
        /// </summary>
        void Update()
        {
            // 每秒向左移动MOVE_SPEED距离
            transform.position += Vector3.left * MOVE_SPEED * Time.deltaTime;
            
            // 检查是否到达或超过目标位置（x <= 0）
            if (transform.position.x <= 0f)
            {
                // 触发到达目标事件
                OnReachTarget?.Invoke(this);
                
                // 自毁
                Destroy(gameObject);
            }
        }
    }
}