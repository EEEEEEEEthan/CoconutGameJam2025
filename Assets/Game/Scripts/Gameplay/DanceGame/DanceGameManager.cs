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
    /// 判定时间窗口（秒）
    /// </summary>
    [SerializeField] private float judgmentWindow = 0.2f;
    
    /// <summary>
    /// 统计计数器
    /// </summary>
    private int correctCount = 0;
    private int wrongCount = 0;
    private int missCount = 0;
    
    /// <summary>
    /// 游戏开始时间
    /// </summary>
    private float gameStartTime;
        
        /// <summary>
    /// 启动跳舞玩法系统
    /// </summary>
    /// <param name="callback">游戏结束时的回调函数，返回游戏统计结果</param>
    public void StartGame(Action<(int correct, int wrong, int miss)> callback)
    {
        gameEndCallback = callback;
        gameStartTime = Time.time;
        
        // 重置统计计数器
        correctCount = 0;
        wrongCount = 0;
        missCount = 0;
        
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
    /// 每帧更新，处理输入检测
    /// </summary>
    void Update()
    {
        // 检测输入
        DetectInput();
    }
    
    /// <summary>
    /// 检测玩家输入
    /// </summary>
    private void DetectInput()
    {
        // 检测常用按键
        KeyCode[] keysToCheck = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.Q, KeyCode.X, KeyCode.Z };
        
        foreach (KeyCode key in keysToCheck)
        {
            if (Input.GetKeyDown(key))
            {
                ProcessInput(key);
            }
        }
    }
    
    /// <summary>
    /// 处理玩家输入
    /// </summary>
    /// <param name="inputKey">按下的按键</param>
    private void ProcessInput(KeyCode inputKey)
    {
        float currentGameTime = Time.time - gameStartTime;
        Note3DModel closestNote = null;
        float closestDistance = float.MaxValue;
        
        // 找到最接近目标位置且按键匹配的音符
        foreach (Note3DModel note in activeNotes)
        {
            if (note.noteData.key == inputKey)
            {
                float timeToTarget = note.noteData.time - currentGameTime;
                float distance = Mathf.Abs(timeToTarget);
                
                if (distance < closestDistance && distance <= judgmentWindow)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }
        
        if (closestNote != null)
        {
            // 正确判定
            TriggerCorrectEvent(closestNote);
            RemoveNote(closestNote);
        }
        else
        {
            // 错误判定（按键错误或时机不对）
            TriggerWrongEvent(inputKey);
        }
    }
    
    /// <summary>
    /// 音符到达目标时的处理
    /// </summary>
    /// <param name="note">到达目标的音符</param>
    private void OnNoteReachTarget(Note3DModel note)
    {
        // Miss判定（音符到达但玩家未按键）
        TriggerMissEvent(note);
        RemoveNote(note);
    }
    
    /// <summary>
    /// 移除音符
    /// </summary>
    /// <param name="note">要移除的音符</param>
    private void RemoveNote(Note3DModel note)
    {
        activeNotes.Remove(note);
        
        // 检查游戏是否结束
        if (activeNotes.Count == 0)
        {
            EndGame();
        }
    }
    
    /// <summary>
    /// 触发正确事件
    /// </summary>
    /// <param name="note">正确判定的音符</param>
    private void TriggerCorrectEvent(Note3DModel note)
    {
        correctCount++;
        Debug.Log($"Correct! Key: {note.noteData.key}, Score: {correctCount}");
    }
    
    /// <summary>
    /// 触发错误事件
    /// </summary>
    /// <param name="inputKey">错误的按键</param>
    private void TriggerWrongEvent(KeyCode inputKey)
    {
        wrongCount++;
        Debug.Log($"Wrong! Key: {inputKey}, Wrong Count: {wrongCount}");
    }
    
    /// <summary>
    /// 触发错过事件
    /// </summary>
    /// <param name="note">错过的音符</param>
    private void TriggerMissEvent(Note3DModel note)
    {
        missCount++;
        Debug.Log($"Miss! Key: {note.noteData.key}, Miss Count: {missCount}");
    }
    
    /// <summary>
    /// 结束游戏
    /// </summary>
    private void EndGame()
    {
        Debug.Log($"Game End! Correct: {correctCount}, Wrong: {wrongCount}, Miss: {missCount}");
        gameEndCallback?.Invoke((correctCount, wrongCount, missCount));
    }
    }
}
