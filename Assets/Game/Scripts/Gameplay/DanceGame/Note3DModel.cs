using System;
using UnityEngine;
using Game.ResourceManagement;

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
        /// 音符的网格渲染器，用于材质属性设置
        /// </summary>
        [SerializeField] private MeshRenderer meshRenderer;
        
        /// <summary>
        /// DanceGameManager的Transform引用，用于本地空间坐标计算
        /// </summary>
        private Transform managerTransform;
        
        /// <summary>
        /// 材质属性块，用于设置纹理
        /// </summary>
        private MaterialPropertyBlock materialPropertyBlock;
        

        
        /// <summary>
        /// 游戏开始时间
        /// </summary>
        private float gameStartTime;
        
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
            gameStartTime = Time.time;
            
            // 设置音符纹理
            SetNoteTexture();
        }
        
        /// <summary>
        /// 根据按键设置音符纹理
        /// </summary>
        private void SetNoteTexture()
        {
            if (meshRenderer == null) return;
            
            // 创建MaterialPropertyBlock
            if (materialPropertyBlock == null)
                materialPropertyBlock = new MaterialPropertyBlock();
            
            // 根据按键获取对应纹理
            Texture2D texture = GetTextureByKey(noteData.key);
            if (texture != null)
            {
                materialPropertyBlock.SetTexture("_MainTex", texture);
                meshRenderer.SetPropertyBlock(materialPropertyBlock);
            }
        }
        
        /// <summary>
        /// 根据按键字符串获取对应纹理
        /// </summary>
        /// <param name="key">按键字符串</param>
        /// <returns>对应的纹理</returns>
        private Texture2D GetTextureByKey(KeyCode key)
        {
            return key switch
            {
                KeyCode.A => ResourceTable.aPng.Main,
                KeyCode.S => ResourceTable.sPng.Main,
                KeyCode.W => ResourceTable.wPng.Main,
                KeyCode.Q => ResourceTable.qPng.Main,
                KeyCode.X => ResourceTable.xPng.Main,
                KeyCode.Z => ResourceTable.zPng.Main,
                _ => null
            };
        }
        
        /// <summary>
        /// 每帧更新音符位置
        /// </summary>
        void Update()
        {
            // 计算当前游戏时间
            float currentGameTime = Time.time - gameStartTime;
            
            // 基于抵达时间和当前时间精确计算位置，避免顿卡造成精度丢失
            // 音符应该在noteData.time时间到达目标位置（x=0）
            float timeRemaining = noteData.time - currentGameTime;
            float targetX = timeRemaining * MOVE_SPEED;
            
            // 更新位置（直接设置x坐标，y和z保持不变）
            Vector3 currentPosition = transform.localPosition;
            currentPosition.x = targetX;
            transform.localPosition = currentPosition;
            
            // 检查是否到达或超过目标位置
            if (timeRemaining <= 0f)
            {
                // 触发到达目标事件
                OnReachTarget?.Invoke(this);
                
                // 自毁
                Destroy(gameObject);
            }
        }
    }
}
