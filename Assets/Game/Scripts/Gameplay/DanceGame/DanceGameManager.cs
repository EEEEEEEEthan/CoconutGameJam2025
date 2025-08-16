using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Game.Gameplay.DanceGame
{
    /// <summary>
    /// 跳舞音乐游戏主控制器
    /// 集成所有管理功能：数据加载、音符生成、输入处理、判定系统、分数统计
    /// </summary>
    public class DanceGameManager : MonoBehaviour
    {
        /// <summary>
        /// 音符3D预制体引用，用于生成音符实例
        /// </summary>
        [SerializeField] Note3DModel note3DPrefab;
        
        /// <summary>
        /// 关卡txt文件资源，包含音符时序数据
        /// </summary>
        [SerializeField] TextAsset levelTextAsset;
        
        /// <summary>
        /// 目标位置（音符到达的位置）
        /// </summary>
        private Vector3 targetPosition = Vector3.zero;
        
        /// <summary>
        /// 生成的音符列表
        /// </summary>
        private List<Note3DModel> activeNotes = new List<Note3DModel>();
        
        /// <summary>
        /// 游戏结束回调
        /// </summary>
        private Action<(int correct, int wrong, int miss)> gameEndCallback;
        
        /// <summary>
        /// 启动跳舞玩法系统
        /// </summary>
        /// <param name="callback">游戏结束时的回调函数，返回游戏统计结果</param>
        public void StartGame(Action<(int correct, int wrong, int miss)> callback)
        {
            gameEndCallback = callback;
            
            if (levelTextAsset == null)
            {
                Debug.LogError("Level text asset is not assigned!");
                return;
            }
            
            if (note3DPrefab == null)
            {
                Debug.LogError("Note3D prefab is not assigned!");
                return;
            }
            
            // 解析txt文件并生成所有音符
            ParseAndGenerateNotes();
        }
        
        /// <summary>
        /// 解析txt文件并生成所有音符
        /// </summary>
        private void ParseAndGenerateNotes()
        {
            string[] lines = levelTextAsset.text.Split('\n');
            
            // 正则表达式匹配格式：[时间]键
            Regex notePattern = new Regex(@"\[([0-9]+\.?[0-9]*)\]([A-Za-z]+)");
            
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;
                
                Match match = notePattern.Match(trimmedLine);
                if (match.Success)
                {
                    string timeStr = match.Groups[1].Value;
                    string keyStr = match.Groups[2].Value;
                    
                    if (float.TryParse(timeStr, out float time) && System.Enum.TryParse<KeyCode>(keyStr, true, out KeyCode keyCode))
                    {
                        NoteData noteData = new NoteData
                        {
                            time = time,
                            key = keyCode
                        };
                        
                        GenerateNote(noteData);
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to parse note data: {trimmedLine}");
                    }
                }
            }
        }
        
        /// <summary>
        /// 生成单个音符
        /// </summary>
        /// <param name="noteData">音符数据</param>
        private void GenerateNote(NoteData noteData)
        {
            // 计算音符的初始位置（在本地空间中）
            // 由于音符需要在noteData.time时间到达目标位置
            // 而移动速度是每秒0.1距离，所以初始位置需要向右偏移
            float distanceToTravel = noteData.time * Note3DModel.MOVE_SPEED;
            Vector3 localStartPosition = targetPosition + Vector3.right * distanceToTravel;
            
            // 实例化音符，设置为DanceGameManager的子对象
            Note3DModel noteInstance = Instantiate(note3DPrefab, this.transform);
            noteInstance.transform.localPosition = localStartPosition;
            
            // 为调试方便，使用按键命名音符GameObject
            noteInstance.gameObject.name = $"Note_{noteData.key}";
            
            noteInstance.Initialize(noteData, this.transform);
            
            // 订阅到达目标事件
            noteInstance.OnReachTarget += OnNoteReachTarget;
            
            // 添加到活跃音符列表
            activeNotes.Add(noteInstance);
        }
        
        /// <summary>
        /// 音符到达目标时的处理
        /// </summary>
        /// <param name="note">到达目标的音符</param>
        private void OnNoteReachTarget(Note3DModel note)
        {
            // 从活跃列表中移除
            activeNotes.Remove(note);
            
            // TODO: 实现判定逻辑和分数统计
        }
    }
}
