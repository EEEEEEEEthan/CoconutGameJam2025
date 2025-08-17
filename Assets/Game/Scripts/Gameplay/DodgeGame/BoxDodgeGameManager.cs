using System.Collections;
using UnityEngine;
using Game.Gameplay;

namespace Game.Gameplay.DodgeGame
{
    /// <summary>
    /// 躲避Box游戏主控制器，管理游戏流程、Box发射和计数系统
    /// </summary>
    public class BoxDodgeGameManager : MonoBehaviour
    {
        [Header("游戏配置")]
        public int requiredDodgeCount = 10;    // 胜利所需躲避数量
        public float launchInterval = 1f;      // Box发射间隔
        
        [Header("发射系统")]
        public Transform[] launcherPositions;  // 发射源位置数组
        public Rect targetArea;                // z=0平面目标区域
        public GameObject boxPrefab;           // Box预制体
        
        [Header("玩家引用")]
        public Player player;                  // 现有玩家组件
        
        [Header("调试设置")]
        public bool showTargetArea = true;     // 是否显示目标区域
        public bool autoStart = true;         // 是否自动开始游戏
        
        private int currentDodgeCount = 0;     // 当前躲避计数
        private int currentLauncherIndex = 0;  // 当前发射器索引
        private bool isGameActive = false;     // 游戏是否激活
        private Coroutine launchCoroutine;    // 发射协程
        
        // 游戏状态事件
        public System.Action<int, int> OnDodgeCountChanged; // 躲避计数改变 (当前, 目标)
        public System.Action OnGameWon;                     // 游戏胜利
        public System.Action OnGameLost;                    // 游戏失败
        
        void Awake()
        {
            // 订阅DodgeBox事件
            DodgeBox.OnBoxHitPlayer += HandleBoxHitPlayer;
            DodgeBox.OnBoxDodged += HandleBoxDodged;
        }
        
        void OnDestroy()
        {
            // 取消订阅事件
            DodgeBox.OnBoxHitPlayer -= HandleBoxHitPlayer;
            DodgeBox.OnBoxDodged -= HandleBoxDodged;
        }
        
        void Start()
        {
            // 验证配置
            if (!ValidateConfiguration())
            {
                Debug.LogError("[BoxDodgeGameManager] 配置验证失败，游戏无法启动");
                return;
            }
            
            if (autoStart)
            {
                StartGame();
            }
        }
        
        /// <summary>
        /// 验证游戏配置
        /// </summary>
        bool ValidateConfiguration()
        {
            if (launcherPositions == null || launcherPositions.Length == 0)
            {
                Debug.LogError("[BoxDodgeGameManager] 发射器位置未配置");
                return false;
            }
            
            if (boxPrefab == null)
            {
                Debug.LogError("[BoxDodgeGameManager] Box预制体未配置");
                return false;
            }
            
            if (player == null)
            {
                Debug.LogError("[BoxDodgeGameManager] Player组件未配置");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            if (isGameActive)
            {
                Debug.LogWarning("[BoxDodgeGameManager] 游戏已经在运行中");
                return;
            }
            
            Debug.Log("[BoxDodgeGameManager] 躲避Box游戏开始！目标：连续躲避 " + requiredDodgeCount + " 个Box");
            
            // 重置游戏状态
            currentDodgeCount = 0;
            currentLauncherIndex = 0;
            isGameActive = true;
            
            // 触发计数更新事件
            OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
            
            // 开始发射Box
            launchCoroutine = StartCoroutine(LaunchBoxCoroutine());
        }
        
        /// <summary>
        /// 停止游戏
        /// </summary>
        public void StopGame()
        {
            if (!isGameActive) return;
            
            isGameActive = false;
            
            if (launchCoroutine != null)
            {
                StopCoroutine(launchCoroutine);
                launchCoroutine = null;
            }
            
            Debug.Log("[BoxDodgeGameManager] 游戏已停止");
        }
        
        /// <summary>
        /// Box发射协程
        /// </summary>
        IEnumerator LaunchBoxCoroutine()
        {
            while (isGameActive)
            {
                LaunchBox();
                yield return new WaitForSeconds(launchInterval);
            }
        }
        
        /// <summary>
        /// 发射一个Box
        /// </summary>
        void LaunchBox()
        {
            if (!isGameActive) return;
            
            // 获取当前发射器位置
            Transform launcher = launcherPositions[currentLauncherIndex];
            
            // 计算目标位置（在目标区域内随机选择一点）
            Vector3 targetPos = CalculateTargetPosition();
            
            // 创建Box
            GameObject boxObj = Instantiate(boxPrefab, launcher.position, Quaternion.identity);
            DodgeBox dodgeBox = boxObj.GetComponent<DodgeBox>();
            
            if (dodgeBox != null)
            {
                // 初始化Box参数
                dodgeBox.Initialize(launcher.position, targetPos, dodgeBox.speed);
                
                // 设置玩家层级（用于碰撞检测）
                if (player != null && player.PlayerPositionTrigger != null)
                {
                    dodgeBox.playerLayer = 1 << player.PlayerPositionTrigger.gameObject.layer;
                }
            }
            
            // 切换到下一个发射器
            currentLauncherIndex = (currentLauncherIndex + 1) % launcherPositions.Length;
            
            Debug.Log($"[BoxDodgeGameManager] 发射Box，目标位置: {targetPos}");
        }
        
        /// <summary>
        /// 计算目标位置
        /// </summary>
        Vector3 CalculateTargetPosition()
        {
            // 在目标区域内随机选择一点
            float randomX = Random.Range(targetArea.xMin, targetArea.xMax);
            float randomY = Random.Range(targetArea.yMin, targetArea.yMax);
            
            return new Vector3(randomX, randomY, 0f);
        }
        
        /// <summary>
        /// 处理Box击中玩家事件
        /// </summary>
        void HandleBoxHitPlayer(DodgeBox box)
        {
            Debug.Log("[BoxDodgeGameManager] 玩家被Box击中！游戏失败，重置计数");
            
            // 重置计数
            currentDodgeCount = 0;
            OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
            OnGameLost?.Invoke();
            
            // 扩展点：可在此处添加失败效果、音效等
            Debug.Log($"游戏失败 - 当前进度: {currentDodgeCount}/{requiredDodgeCount}");
        }
        
        /// <summary>
        /// 处理Box被成功躲避事件
        /// </summary>
        void HandleBoxDodged(DodgeBox box)
        {
            currentDodgeCount++;
            Debug.Log($"[BoxDodgeGameManager] 成功躲避Box！当前进度: {currentDodgeCount}/{requiredDodgeCount}");
            
            OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
            
            // 检查是否达到胜利条件
            if (currentDodgeCount >= requiredDodgeCount)
            {
                HandleGameWon();
            }
        }
        
        /// <summary>
        /// 处理游戏胜利
        /// </summary>
        void HandleGameWon()
        {
            Debug.Log("[BoxDodgeGameManager] 恭喜！成功完成躲避挑战！");
            
            StopGame();
            OnGameWon?.Invoke();
            
            // 扩展点：可在此处添加胜利效果、奖励等
            Debug.Log($"游戏胜利 - 成功躲避: {currentDodgeCount}/{requiredDodgeCount}");
        }
        
        /// <summary>
        /// 重置游戏
        /// </summary>
        public void ResetGame()
        {
            StopGame();
            currentDodgeCount = 0;
            OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
            Debug.Log("[BoxDodgeGameManager] 游戏已重置");
        }
        
        void OnDrawGizmos()
        {
            if (!showTargetArea) return;
            
            // 绘制目标区域
            Gizmos.color = Color.green;
            Vector3 center = new Vector3(targetArea.center.x, targetArea.center.y, 0f);
            Vector3 size = new Vector3(targetArea.width, targetArea.height, 0.1f);
            Gizmos.DrawWireCube(center, size);
            
            // 绘制发射器位置
            if (launcherPositions != null)
            {
                Gizmos.color = Color.red;
                foreach (Transform launcher in launcherPositions)
                {
                    if (launcher != null)
                    {
                        Gizmos.DrawWireSphere(launcher.position, 0.5f);
                    }
                }
            }
        }
    }
}