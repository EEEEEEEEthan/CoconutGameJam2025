using System;
using UnityEngine;

namespace Game.Gameplay.DanceGame
{
    /// <summary>
    /// 跳舞音乐游戏主控制器
    /// 集成所有管理功能：数据加载、音符生成、输入处理、判定系统、分数统计
    /// </summary>
    public class DanceGameManager : MonoBehaviour
    {
        [SerializeField] Note3DModel note3DPrefab;
        
        /// <summary>
        /// 启动跳舞玩法系统
        /// </summary>
        /// <param name="callback">游戏结束时的回调函数，返回游戏统计结果</param>
        public void Start(Action<(int correct, int wrong, int miss)> callback)
        {
            // TODO: 实现游戏启动逻辑
        }
    }
}