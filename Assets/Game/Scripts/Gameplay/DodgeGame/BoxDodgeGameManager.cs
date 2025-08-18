using System.Collections;
using UnityEngine;
namespace Game.Gameplay.DodgeGame
{
	public class BoxDodgeGameManager : GameBehaviour
	{
		[Header("游戏配置")] public int requiredDodgeCount = 10;
		public float launchInterval = 1f;
		[Header("发射系统")] public Transform[] launcherPositions;
		public Rect targetArea;
		public GameObject boxPrefab;
		[Header("玩家引用")] [Header("空气墙")] public GameObject airWallLeft;
		public GameObject airWallRight;
		[Header("调试设置")] public bool showTargetArea = true;
		public bool autoStart = true;
		public System.Action<int, int> OnDodgeCountChanged;
		public System.Action OnGameWon;
		public System.Action OnGameLost;
		int currentDodgeCount;
		int currentLauncherIndex;
		bool isGameActive;
		Coroutine launchCoroutine;
		void Awake()
		{
			DodgeBox.OnBoxHitPlayer += HandleBoxHitPlayer;
			DodgeBox.OnBoxDodged += HandleBoxDodged;
			if (boxPrefab != null) boxPrefab.SetActive(false);
		}
		void Start()
		{
			if (!ValidateConfiguration())
			{
				Debug.LogError("[BoxDodgeGameManager] 配置验证失败，游戏无法启动");
				return;
			}
			if (autoStart) StartGame();
		}
		void OnDestroy()
		{
			DodgeBox.OnBoxHitPlayer -= HandleBoxHitPlayer;
			DodgeBox.OnBoxDodged -= HandleBoxDodged;
		}
		void OnDrawGizmos()
		{
			if (!showTargetArea) return;
			Gizmos.color = Color.green;
			var localCenter = new Vector3(targetArea.center.x, targetArea.center.y, 0f);
			var worldCenter = transform.TransformPoint(localCenter);
			var size = new Vector3(targetArea.width, targetArea.height, 0.1f);
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(localCenter, size);
			Gizmos.matrix = Matrix4x4.identity;
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
			if (airWallLeft != null) airWallLeft.SetActive(true);
			if (airWallRight != null) airWallRight.SetActive(true);
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
		public void ResetGame()
		{
			StopGame();
			currentDodgeCount = 0;
			OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
			Debug.Log("[BoxDodgeGameManager] 游戏已重置");
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
			if (GameRoot.Player == null)
			{
				Debug.LogError("[BoxDodgeGameManager] Player组件未找到");
				return false;
			}
			return true;
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
			var launcher = launcherPositions[currentLauncherIndex];
			var targetPos = CalculateTargetPosition();
			var boxObj = Instantiate(boxPrefab, launcher.position, Quaternion.identity);
			boxObj.SetActive(true);
			var dodgeBox = boxObj.GetComponent<DodgeBox>();
			if (dodgeBox != null)
			{
				dodgeBox.Initialize(launcher.position, targetPos, dodgeBox.speed);
				if (GameRoot.Player != null && GameRoot.Player.PlayerPositionTrigger != null)
					dodgeBox.playerLayer = 1 << GameRoot.Player.PlayerPositionTrigger.gameObject.layer;
			}
			currentLauncherIndex = (currentLauncherIndex + 1) % launcherPositions.Length;
			Debug.Log($"[BoxDodgeGameManager] 发射Box，目标位置: {targetPos}");
		}
		Vector3 CalculateTargetPosition()
		{
			var randomX = Random.Range(targetArea.xMin, targetArea.xMax);
			var randomY = Random.Range(targetArea.yMin, targetArea.yMax);
			var localPos = new Vector3(randomX, randomY, 0f);
			return transform.TransformPoint(localPos);
		}
		void HandleBoxHitPlayer(DodgeBox box)
		{
			if (currentDodgeCount >= requiredDodgeCount) return;
			Debug.Log("[BoxDodgeGameManager] 玩家被Box击中！游戏失败，重置计数");
			currentDodgeCount = 0;
			OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
			OnGameLost?.Invoke();
			Debug.Log($"游戏失败 - 当前进度: {currentDodgeCount}/{requiredDodgeCount}");
		}
		void HandleBoxDodged(DodgeBox box)
		{
			if (currentDodgeCount >= requiredDodgeCount) return;
			currentDodgeCount++;
			Debug.Log($"[BoxDodgeGameManager] 成功躲避Box！当前进度: {currentDodgeCount}/{requiredDodgeCount}");
			OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
			if (currentDodgeCount >= requiredDodgeCount) HandleGameWon();
		}
		void HandleGameWon()
		{
			if (!isGameActive) return;
			Debug.Log("[BoxDodgeGameManager] 恭喜！成功完成躲避挑战！");
			StopGame();
			if (airWallLeft != null) airWallLeft.SetActive(false);
			if (airWallRight != null) airWallRight.SetActive(false);
			OnGameWon?.Invoke();
			Debug.Log($"游戏胜利 - 成功躲避: {currentDodgeCount}/{requiredDodgeCount}");
		}
	}
}
