using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.DanceGame
{
    /// <summary>
    /// 音符检测区域脚本
    /// 当音符进入时变红色，离开时变白色
    /// 只有音符在检测区域内时按对应键才正确
    /// </summary>
    public class NoteDetector : MonoBehaviour
    {
        /// <summary>
        /// 音符进入检测区域事件
        /// </summary>
        public static System.Action<Note3DModel> OnNoteEnter;
        
        /// <summary>
        /// 音符离开检测区域事件
        /// </summary>
        public static System.Action<Note3DModel> OnNoteExit;
        
        [SerializeField] private MeshRenderer meshRenderer;
        private List<Note3DModel> notesInArea = new List<Note3DModel>();
        
        void Awake()
        {
            // 确保有MeshRenderer组件
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
            
            // 确保有Collider组件且设置为Trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
            
            // 创建材质副本，避免影响原始材质
            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            var note = other.GetComponent<Note3DModel>();
            if (note != null && !notesInArea.Contains(note))
            {
                notesInArea.Add(note);
                SetColor(Color.red);
                
                // 触发音符进入事件
                OnNoteEnter?.Invoke(note);
                
                Debug.Log($"音符 {note.name} 进入检测区域", this);
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            var note = other.GetComponent<Note3DModel>();
            if (note != null && notesInArea.Contains(note))
            {
                notesInArea.Remove(note);
                
                // 如果没有音符在区域内，恢复白色
                if (notesInArea.Count == 0)
                {
                    SetColor(Color.white);
                }
                
                // 触发音符离开事件
                OnNoteExit?.Invoke(note);
                
                Debug.Log($"音符 {note.name} 离开检测区域", this);
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
        /// 设置检测区域颜色
        /// </summary>
        /// <param name="color">目标颜色</param>
        private void SetColor(Color color)
        {
            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                meshRenderer.sharedMaterial.color = color;
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