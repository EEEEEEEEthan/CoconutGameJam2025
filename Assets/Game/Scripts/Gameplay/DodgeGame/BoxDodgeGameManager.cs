using System.Collections;
using UnityEngine;
using Game.Gameplay;
namespace Game.Gameplay.DodgeGame
{
	public class BoxDodgeGameManager : MonoBehaviour
	{
		[Header("游戏配置")]
		public int requiredDodgeCount = 10;
		public float launchInterval = 1f;
		[Header("发射系统")]
		public Transform[] launcherPositions;
		public Rect targetArea;
		public GameObject boxPrefab;
		[Header("玩家引用")]
		public Player player;
		[Header("调试设置")]
		public bool showTargetArea = true;
		public bool autoStart = true;
		private int currentDodgeCount = 0;
		private int currentLauncherIndex = 0;
		private bool isGameActive = false;
		private Coroutine launchCoroutine;
		public System.Action<int, int> OnDodgeCountChanged;
		public System.Action OnGameWon;
		public System.Action OnGameLost;
        
		void Awake()
{
DodgeBox.OnBoxHitPlayer += HandleBoxHitPlayer;
DodgeBox.OnBoxDodged += HandleBoxDodged;
if (boxPrefab != null)
{
boxPrefab.SetActive(false);
}
}
		void OnDestroy()
		{
			DodgeBox.OnBoxHitPlayer -= HandleBoxHitPlayer;
			DodgeBox.OnBoxDodged -= HandleBoxDodged;
		}
		void Start()
		{
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
        
		public void StartGame()
		{
			if (isGameActive)
			{
				Debug.LogWarning("[BoxDodgeGameManager] 游戏已经在运行中");
				return;
			}
			Debug.Log("[BoxDodgeGameManager] 躲避Box游戏开始！目标：连续躲避 " + requiredDodgeCount + " 个Box");
			currentDodgeCount = 0;
			currentLauncherIndex = 0;
			isGameActive = true;
			OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
			launchCoroutine = StartCoroutine(LaunchBoxCoroutine());
		}
        
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
        
		IEnumerator LaunchBoxCoroutine()
		{
			while (isGameActive)
			{
				LaunchBox();
				yield return new WaitForSeconds(launchInterval);
			}
		}
        
		void LaunchBox()
		{
			if (!isGameActive) return;
			Transform launcher = launcherPositions[currentLauncherIndex];
			Vector3 targetPos = CalculateTargetPosition();
			GameObject boxObj = Instantiate(boxPrefab, launcher.position, Quaternion.identity);
boxObj.SetActive(true);
DodgeBox dodgeBox = boxObj.GetComponent<DodgeBox>();
			if (dodgeBox != null)
			{
				dodgeBox.Initialize(launcher.position, targetPos, dodgeBox.speed);
				if (player != null && player.PlayerPositionTrigger != null)
				{
					dodgeBox.playerLayer = 1 << player.PlayerPositionTrigger.gameObject.layer;
				}
			}
			currentLauncherIndex = (currentLauncherIndex + 1) % launcherPositions.Length;
			Debug.Log($"[BoxDodgeGameManager] 发射Box，目标位置: {targetPos}");
		}
        
		Vector3 CalculateTargetPosition()
{
float randomX = Random.Range(targetArea.xMin, targetArea.xMax);
float randomY = Random.Range(targetArea.yMin, targetArea.yMax);
Vector3 localPos = new Vector3(randomX, randomY, 0f);
return transform.TransformPoint(localPos);
}
        
		void HandleBoxHitPlayer(DodgeBox box)
		{
			Debug.Log("[BoxDodgeGameManager] 玩家被Box击中！游戏失败，重置计数");
			currentDodgeCount = 0;
			OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
			OnGameLost?.Invoke();
			Debug.Log($"游戏失败 - 当前进度: {currentDodgeCount}/{requiredDodgeCount}");
		}
        
		void HandleBoxDodged(DodgeBox box)
		{
			currentDodgeCount++;
			Debug.Log($"[BoxDodgeGameManager] 成功躲避Box！当前进度: {currentDodgeCount}/{requiredDodgeCount}");
			OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
			if (currentDodgeCount >= requiredDodgeCount)
			{
				HandleGameWon();
			}
		}
        
		void HandleGameWon()
		{
			Debug.Log("[BoxDodgeGameManager] 恭喜！成功完成躲避挑战！");
			StopGame();
			OnGameWon?.Invoke();
			Debug.Log($"游戏胜利 - 成功躲避: {currentDodgeCount}/{requiredDodgeCount}");
		}
        
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
Gizmos.color = Color.green;
Vector3 localCenter = new Vector3(targetArea.center.x, targetArea.center.y, 0f);
Vector3 worldCenter = transform.TransformPoint(localCenter);
Vector3 size = new Vector3(targetArea.width, targetArea.height, 0.1f);
Gizmos.matrix = transform.localToWorldMatrix;
Gizmos.DrawWireCube(localCenter, size);
Gizmos.matrix = Matrix4x4.identity;
}
	}
}