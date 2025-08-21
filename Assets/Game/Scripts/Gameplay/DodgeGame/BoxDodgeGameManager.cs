using System;
using System.Collections;
using UnityEngine;
namespace Game.Gameplay.DodgeGame
{
	public class BoxDodgeGameManager : GameBehaviour
	{
		[Header("游戏配置")] public int requiredDodgeCount = 10;
		public float launchInterval = 1f; // 每次成功发射后的间隔（不含动画前置延迟）
		// 发射系统: 由 boy / girl 的 Animator 触发 Launch 动画，再延迟 0.3s 发射
		public Rect targetArea;
		public GameObject boxPrefab;
		[Header("玩家引用")] [Header("空气墙")] public GameObject airWallLeft;
		public GameObject airWallRight;
		[Header("调试设置")] public bool showTargetArea = true;
		public bool autoStart = true;
		[SerializeField] Animator boy;
		[SerializeField] Animator girl;
		[SerializeField] Transform lookTarget;
		[Header("动画触发参数")] [SerializeField] string launchTriggerName = "Launch"; // 跳跃改为物理位移，不再使用动画 Trigger
		[SerializeField] string shyTriggerName = "Shy";
		[Header("时间参数")] [SerializeField] float startRotateDuration = 0.5f;
		[SerializeField] float endRotateDuration = 0.6f;
		[SerializeField] float launchAnimDelay = 0.3f; // 与协程内部常量保持一致，可在 Inspector 调整
		[SerializeField] float jumpToRotateDelay = 0.05f;
		[Header("受击反馈")] [SerializeField] float hitColorRecoverDuration = 1.2f; // 玩家被击中后从红色恢复到白色的时长
		[Header("跳跃参数")] [SerializeField] float startJumpHeight = 0.08f;
		[SerializeField] float startJumpDuration = 0.25f;
		[SerializeField] float endJumpHeight = 0.08f;
		[SerializeField] float endJumpDuration = 0.25f;
		[SerializeField] Transform look2;
		public Action<int, int> OnDodgeCountChanged;
		public Action OnGameWon;
		public Action OnGameLost;
		int currentDodgeCount;
		int currentLauncherIndex;
		bool isGameActive;
		bool startSequenceCompleted;
		Coroutine launchCoroutine;
		Coroutine gameFlowCoroutine;
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
			GameRoot.GameCanvas.Filmic(true);
			GameRoot.CameraController.LookAt(lookTarget, 15f, 0.2f);
			currentDodgeCount = 0;
			currentLauncherIndex = 0;
			isGameActive = true;
			startSequenceCompleted = false;
			if (airWallLeft != null) airWallLeft.SetActive(true);
			if (airWallRight != null) airWallRight.SetActive(true);
			OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
			if (gameFlowCoroutine != null) StopCoroutine(gameFlowCoroutine);
			gameFlowCoroutine = StartCoroutine(GameFlowCoroutine());
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
			if (gameFlowCoroutine != null)
			{
				StopCoroutine(gameFlowCoroutine);
				gameFlowCoroutine = null;
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
			if (boy == null || girl == null)
			{
				Debug.LogError("[BoxDodgeGameManager] boy 或 girl Animator 未配置");
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
			// 循环：交替角色 -> 触发动画 -> 等待动画前置时间 -> 发射 -> 等待发射间隔
			while (isGameActive)
			{
				var launcherAnimator = currentLauncherIndex == 0 ? boy : girl;
				if (launcherAnimator != null)
				{
					launcherAnimator.SetTrigger(launchTriggerName);
					yield return new WaitForSeconds(launchAnimDelay);
					LaunchBox(launcherAnimator.transform, launcherAnimator == boy ? "Boy" : "Girl");
				}
				currentLauncherIndex = 1 - currentLauncherIndex; // 交替
				yield return new WaitForSeconds(launchInterval);
			}
		}
		IEnumerator GameFlowCoroutine()
		{
			// 1. 开始序列（跳跃 + 朝向玩家）
			yield return StartCoroutine(StartSequence());
			startSequenceCompleted = true;
			// 2. 进入发射循环
			if (isGameActive) launchCoroutine = StartCoroutine(LaunchBoxCoroutine());
		}
		IEnumerator StartSequence()
		{
			// 同时原地小跳（使用 Transform 扩展 Jump）
			var boyDone = boy == null;
			var girlDone = girl == null;
			if (boy != null) boy.transform.Jump(boy.transform.position, startJumpHeight, startJumpDuration, () => boyDone = true);
			if (girl != null) girl.transform.Jump(girl.transform.position, startJumpHeight, startJumpDuration, () => girlDone = true);
			while (!boyDone || !girlDone) yield return null;
			yield return new WaitForSeconds(jumpToRotateDelay);
			if (GameRoot.Player == null) yield break;
			var playerPos = GameRoot.Player.transform.position;
			var t = 0f;
			var boyStartRot = boy != null ? boy.transform.rotation : Quaternion.identity;
			var girlStartRot = girl != null ? girl.transform.rotation : Quaternion.identity;
			var boyTargetRot = boyStartRot;
			var girlTargetRot = girlStartRot;
			if (boy != null)
			{
				var dir = playerPos - boy.transform.position;
				dir.y = 0f;
				if (dir.sqrMagnitude > 0.0001f) boyTargetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
			}
			if (girl != null)
			{
				var dir = playerPos - girl.transform.position;
				dir.y = 0f;
				if (dir.sqrMagnitude > 0.0001f) girlTargetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
			}
			while (t < 1f && isGameActive)
			{
				t += Time.deltaTime / Mathf.Max(0.0001f, startRotateDuration);
				if (boy != null) boy.transform.rotation = Quaternion.Slerp(boyStartRot, boyTargetRot, t);
				if (girl != null) girl.transform.rotation = Quaternion.Slerp(girlStartRot, girlTargetRot, t);
				yield return null;
			}
		}
		IEnumerator EndSequence()
		{
			if (boy == null || girl == null) yield break;
			yield return new WaitForSeconds(1);
			GameRoot.CameraController.LookAt(look2, 13f);
			yield return new WaitForSeconds(0.5f);
			GameRoot.Player.SmoothSetDissolve(1, 0.2f);
			yield return new WaitForSeconds(0.5f);
			// 同时小跳
			bool boyDone = false, girlDone = false;
			boy.transform.Jump(boy.transform.position, endJumpHeight, endJumpDuration, () => boyDone = true);
			girl.transform.Jump(girl.transform.position, endJumpHeight, endJumpDuration, () => girlDone = true);
			while (!boyDone || !girlDone) yield return null;
			yield return new WaitForSeconds(jumpToRotateDelay);
			var boyStartRot = boy.transform.rotation;
			var girlStartRot = girl.transform.rotation;
			var dirBoy = girl.transform.position - boy.transform.position;
			dirBoy.y = 0f;
			if (dirBoy.sqrMagnitude < 0.0001f) dirBoy = boy.transform.forward;
			var dirGirl = boy.transform.position - girl.transform.position;
			dirGirl.y = 0f;
			if (dirGirl.sqrMagnitude < 0.0001f) dirGirl = girl.transform.forward;
			var boyTargetRot = Quaternion.LookRotation(dirBoy.normalized, Vector3.up);
			var girlTargetRot = Quaternion.LookRotation(dirGirl.normalized, Vector3.up);
			var t = 0f;
			while (t < 1f)
			{
				t += Time.deltaTime / Mathf.Max(0.0001f, endRotateDuration);
				boy.transform.rotation = Quaternion.Slerp(boyStartRot, boyTargetRot, t);
				girl.transform.rotation = Quaternion.Slerp(girlStartRot, girlTargetRot, t);
				yield return null;
			}
			if (!string.IsNullOrEmpty(shyTriggerName))
			{
				boy.SetTrigger(shyTriggerName);
				girl.SetTrigger(shyTriggerName);
			}
			GameRoot.Player.Unlock(KeyCode.Alpha3, true);
			GameRoot.GameCanvas.Filmic(false);
			yield return new WaitForSeconds(1);
			GameRoot.Player.SmoothSetDissolve(0, 0.2f);
			GameRoot.CameraController.LookAtPlayer();
			OnGameWon?.Invoke();
		}
		void LaunchBox(Transform launcherTransform, string launcherLabel)
		{
			if (!isGameActive) return;
			var targetPos = CalculateTargetPosition();
			var boxObj = Instantiate(boxPrefab, launcherTransform.position + Vector3.up * 0.1f, Quaternion.identity);
			boxObj.SetActive(true);
			var dodgeBox = boxObj.GetComponent<DodgeBox>();
			if (dodgeBox != null)
			{
				dodgeBox.Initialize(launcherTransform.position + Vector3.up * 0.1f, targetPos, dodgeBox.speed);
				if (GameRoot.Player != null && GameRoot.Player.PlayerPositionTrigger != null)
					dodgeBox.playerLayer = 1 << GameRoot.Player.PlayerPositionTrigger.gameObject.layer;
			}
			Debug.Log($"[BoxDodgeGameManager] {launcherLabel} 发射 Box，目标位置: {targetPos}");
		}
		Vector3 CalculateTargetPosition()
		{
			var randomX = UnityEngine.Random.Range(targetArea.xMin, targetArea.xMax);
			var randomY = UnityEngine.Random.Range(targetArea.yMin, targetArea.yMax);
			var localPos = new Vector3(randomX, randomY, 0f);
			return transform.TransformPoint(localPos);
		}
		void HandleBoxHitPlayer(DodgeBox box)
		{
			if (currentDodgeCount >= requiredDodgeCount) return;
			// 玩家受击颜色反馈：立刻变红 -> 平滑回到白色
			if (GameRoot.Player != null)
			{
				var p = GameRoot.Player;
				p.SmoothSetMaterialColor(Color.red, 0f, () => p.SmoothSetMaterialColor(Color.white, hitColorRecoverDuration));
			}
			// 令命中玩家的 box 变红后溶解
			if (box != null)
			{
				box.SetColor(Color.red);
				box.StartDissolve();
			}
			// 屏幕震动
			GameRoot.CameraController?.Shake(0.25f);
			Debug.Log("[BoxDodgeGameManager] 玩家被Box击中！游戏失败，重置计数");
			currentDodgeCount = 0;
			OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
			OnGameLost?.Invoke();
			Debug.Log($"游戏失败 - 当前进度: {currentDodgeCount}/{requiredDodgeCount}");
		}
		void HandleBoxDodged(DodgeBox box)
		{
			if (currentDodgeCount >= requiredDodgeCount) return;
			// 躲避成功：该 box 变蓝并溶解
			if (box != null)
			{
				box.SetColor(new Color(0.5f, 0.7f, 1f));
				box.StartDissolve();
			}
			currentDodgeCount++;
			Debug.Log($"[BoxDodgeGameManager] 成功躲避Box！当前进度: {currentDodgeCount}/{requiredDodgeCount}");
			OnDodgeCountChanged?.Invoke(currentDodgeCount, requiredDodgeCount);
			if (currentDodgeCount >= requiredDodgeCount) HandleGameWon();
		}
		void HandleGameWon()
		{
			if (!isGameActive) return;
			Debug.Log("[BoxDodgeGameManager] 恭喜！成功完成躲避挑战！");
			// 停止发射循环
			StopGame();
			// 游戏结束瞬间：溶解场景中所有仍然存在的 Box
			var boxes = UnityEngine.Object.FindObjectsByType<DodgeBox>(FindObjectsSortMode.None);
			foreach (var b in boxes)
			{
				b.StartDissolve();
			}
			if (airWallLeft != null) airWallLeft.SetActive(false);
			if (airWallRight != null) airWallRight.SetActive(false);
			// 执行结束表现（朝向彼此 + Shy）
			StartCoroutine(EndSequence());
			Debug.Log($"游戏胜利 - 成功躲避: {currentDodgeCount}/{requiredDodgeCount}");
		}
	}
}
