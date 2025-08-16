namespace Game.Gameplay.DanceGame
{
    /// <summary>
    /// 判定类型枚举
    /// 定义按键判定的不同结果类型
    /// </summary>
    public enum JudgmentType
    {
        /// <summary>
        /// 完美判定
        /// </summary>
        Perfect,
        
        /// <summary>
        /// 良好判定
        /// </summary>
        Good,
        
        /// <summary>
        /// 未按键或时机不准确
        /// </summary>
        Miss,
        
        /// <summary>
        /// 按错键
        /// </summary>
        Wrong
    }
}