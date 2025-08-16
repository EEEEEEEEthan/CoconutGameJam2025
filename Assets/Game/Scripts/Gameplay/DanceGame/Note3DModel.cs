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
        /// DanceGameManager的Transform引用，用于本地空间坐标计算
        /// </summary>
        private Transform managerTransform;
        
        /// <summary>
        /// 到达目标点时的事件回调
        /// </summary>
        public System.Action<Note3DModel> OnReachTarget;
        
        /// <summary>
        /// 初始化音符数据
        /// </summary>
        /// <param name="data">音符数据</param>
        /// <param name="manager">DanceGameManager的Transform</param>
        public void Initialize(NoteData data, Transform manager)
        {
            noteData = data;
            managerTransform = manager;
        }
        
        /// <summary>
        /// 每帧更新音符位置
        /// </summary>
        void Update()
        {
            // 在本地空间中每秒向左移动MOVE_SPEED距离
            transform.localPosition += Vector3.left * MOVE_SPEED * Time.deltaTime;
            
            // 检查是否到达或超过目标位置（本地坐标x <= 0）
            if (transform.localPosition.x <= 0f)
            {
                // 触发到达目标事件
                OnReachTarget?.Invoke(this);
                
                // 自毁
                Destroy(gameObject);
            }
        }
    }
}