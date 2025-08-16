using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.DanceGame
{
    /// <summary>
    /// 音符检测区域组件
    /// 当音符进入检测区域时变红色，离开时变白色
    /// 只有音符在检测区域内时按对应键才会被判定为正确
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MeshRenderer))]
    public class NoteDetector : MonoBehaviour
    {
        /// <summary>
        /// 渲染器组件引用
        /// </summary>
        [SerializeField] private MeshRenderer meshRenderer;
        
        /// <summary>
        /// 当前在检测区域内的音符列表
        /// </summary>
        private List<Note3DModel> notesInArea = new List<Note3DModel>();
        
        /// <summary>
        /// 原始材质颜色（白色）
        /// </summary>
        private Color originalColor = Color.white;
        
        /// <summary>
        /// 检测到音符时的颜色（红色）
        /// </summary>
        private Color detectedColor = Color.red;
        
        /// <summary>
        /// 材质属性块，用于动态修改材质颜色
        /// </summary>
        private MaterialPropertyBlock materialPropertyBlock;
        
        /// <summary>
        /// DanceGameManager引用，用于通知判定系统
        /// </summary>
        private DanceGameManager danceGameManager;
        
        void Awake()
        {
            // 获取组件引用
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
            
            // 初始化材质属性块
            materialPropertyBlock = new MaterialPropertyBlock();
            
            // 确保Collider是触发器
            Collider collider = GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;
            
            // 查找DanceGameManager
            danceGameManager = FindObjectOfType<DanceGameManager>();
        }
        
        void Start()
        {
            // 设置初始颜色为白色
            SetColor(originalColor);
        }
        
        /// <summary>
        /// 音符进入检测区域
        /// </summary>
        /// <param name="other">进入的碰撞体</param>
        void OnTriggerEnter(Collider other)
        {
            Note3DModel note = other.GetComponent<Note3DModel>();
            if (note != null && !notesInArea.Contains(note))
            {
                notesInArea.Add(note);
                UpdateColor();
                
                // 通知DanceGameManager音符进入检测区域
                if (danceGameManager != null)
                {
                    danceGameManager.OnNoteEnterDetectionArea(note, this);
                }
                
                Debug.Log($"音符 {note.name} 进入检测区域 {name}", this);
            }
        }
        
        /// <summary>
        /// 音符离开检测区域
        /// </summary>
        /// <param name="other">离开的碰撞体</param>
        void OnTriggerExit(Collider other)
        {
            Note3DModel note = other.GetComponent<Note3DModel>();
            if (note != null && notesInArea.Contains(note))
            {
                notesInArea.Remove(note);
                UpdateColor();
                
                // 通知DanceGameManager音符离开检测区域
                if (danceGameManager != null)
                {
                    danceGameManager.OnNoteExitDetectionArea(note, this);
                }
                
                Debug.Log($"音符 {note.name} 离开检测区域 {name}", this);
            }
        }
        
        /// <summary>
        /// 更新检测区域的颜色
        /// </summary>
        private void UpdateColor()
        {
            Color targetColor = notesInArea.Count > 0 ? detectedColor : originalColor;
            SetColor(targetColor);
        }
        
        /// <summary>
        /// 设置材质颜色
        /// </summary>
        /// <param name="color">目标颜色</param>
        private void SetColor(Color color)
        {
            if (meshRenderer != null && materialPropertyBlock != null)
            {
                materialPropertyBlock.SetColor("_Color", color);
                meshRenderer.SetPropertyBlock(materialPropertyBlock);
            }
        }
        
        /// <summary>
        /// 检查指定音符是否在检测区域内
        /// </summary>
        /// <param name="note">要检查的音符</param>
        /// <returns>如果音符在区域内返回true</returns>
        public bool IsNoteInArea(Note3DModel note)
        {
            return notesInArea.Contains(note);
        }
        
        /// <summary>
        /// 获取当前在检测区域内的所有音符
        /// </summary>
        /// <returns>音符列表</returns>
        public List<Note3DModel> GetNotesInArea()
        {
            return new List<Note3DModel>(notesInArea);
        }
        
        /// <summary>
        /// 清理已销毁的音符引用
        /// </summary>
        public void CleanupDestroyedNotes()
        {
            notesInArea.RemoveAll(note => note == null);
            UpdateColor();
        }
    }
}