using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.FingerRigging;
using Game.Utilities;
using Game.Utilities.UnityTools;
using UnityEngine;
// For Player
// For LegPoseCode
namespace Game.Gameplay.DanceGame
{
	public class DanceGameManager : GameBehaviour
	{
		static readonly int DissolveID = Shader.PropertyToID("_Dissolve");
		[SerializeField] Note3DModel note3DPrefab;
		[SerializeField] TextAsset levelTextAsset;
		[SerializeField] NoteDetector noteDetector;
		[SerializeField] DanceNPC danceNPC;
		[SerializeField] Player player; // 用于监听玩家动作/表情的事件
		[SerializeField] Rigidbody[] rigidbodies;
		[SerializeField] Transform[] unimportant;
		[SerializeField] float unimportantMoveSpeed = 0.01f; // units per second to move left
		[SerializeField] MeshRenderer backgroundMeshRenderer; // 背景网格渲染器
		[SerializeField] string dissolvePropertyName = "_Dissolve"; // 材质溶解属性名
		[SerializeField] ParticleSystem particle;
		[SerializeField] GameObject splashTrigger;
		[SerializeField] ParticleSystem finalParticle;
		[SerializeField] Transform lookHere;
		readonly Vector3 targetPosition = Vector3.zero;
		readonly List<Note3DModel> activeNotes = new();
		Coroutine dissolveCoroutine; // 当前溶解协程
		Action<(int correct, int wrong, int miss)> gameEndCallback;
		int correctCount;
		int wrongCount;
		int missCount;
		float gameStartTime;
		void Awake()
		{
			try
			{
				backgroundMeshRenderer.sharedMaterial = new(backgroundMeshRenderer.sharedMaterial);
				noteDetector.gameObject.SetActive(false);
				note3DPrefab.gameObject.SetActive(false);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		void Update()
		{
			lookHere.transform.position = (GameRoot.Player.transform.position + danceNPC.transform.position) * 0.5f + Vector3.up * 0.1f;

			// 取消直接按键轮询，改为事件驱动（Player/HandIKInput）
			// Move "unimportant" transforms left each frame after OnEnable at configured speed
			if (unimportant != null)
			{
				var delta = Vector3.left * (unimportantMoveSpeed * Time.deltaTime);
				for (var i = 0; i < unimportant.Length; i++)
				{
					var t = unimportant[i];
					if (t != null) t.position += delta;
				}
			}
			foreach (var rigidobody in rigidbodies)
				if (!rigidobody.useGravity)
					rigidobody.AddForce(Vector3.up * 0.001f);
		}
		async void OnEnable()
		{
			try
			{
				lookHere.transform.position = (GameRoot.Player.transform.position + danceNPC.transform.position) * 0.5f + Vector3.up * 0.1f;
				GameRoot.CameraController.LookAt(lookHere, 15f);
				// 开局隐藏所有提示
				HideAllHintsAtStart();
				GetComponent<AudioSource>().Play();
				noteDetector.gameObject.SetActive(true);
				// 确保 player 引用存在（若未在 Inspector 赋值，则尝试自动查找）
				if (player == null)
				{
#if UNITY_2023_1_OR_NEWER
					var root = FindFirstObjectByType<GameRoot>();
#else
					var root = UnityEngine.Object.FindObjectOfType<GameRoot>();
#endif
					player = root ? root.Player : null;
				}
				SubscribePlayerInputEvents(true);
				gameStartTime = Time.time;
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
				NoteDetector.OnNoteEnter += OnNoteEnterDetectionArea;
				NoteDetector.OnNoteExit += OnNoteExitDetectionArea;
				ParseAndGenerateNotes();
				if (danceNPC != null)
				{
					var noteDataList = Parse();
					danceNPC.Dance(noteDataList);
				}
				await MainThreadTimerManager.Await(10);
				SmoothSetDissolve(1, 30f);
				await MainThreadTimerManager.Await(10);
				splashTrigger.SetActive(true);
				foreach (var rigidobody in rigidbodies)
				{
					rigidobody.useGravity = false;
					await MainThreadTimerManager.Await(1);
				}
				finalParticle.Play();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		void OnDisable()
		{
			NoteDetector.OnNoteEnter -= OnNoteEnterDetectionArea;
			NoteDetector.OnNoteExit -= OnNoteExitDetectionArea;
			if (danceNPC != null) danceNPC.StopDance();
			SubscribePlayerInputEvents(false);
		}
		public void SetGameEndCallback(Action<(int correct, int wrong, int miss)> callback) => gameEndCallback = callback;
		/// <summary>
		///     平滑设置背景材质的 _Dissolve 参数。
		///     如果 duration 小于等于 0 则立即设置。
		/// </summary>
		/// <param name="targetValue">目标值 (通常 0~1)</param>
		/// <param name="duration">过渡时长（秒）</param>
		public void SmoothSetDissolve(float targetValue, float duration)
		{
			if (backgroundMeshRenderer == null) return;
			var mat = backgroundMeshRenderer.material; // 实例材质
			if (mat == null) return;
			// 兼容自定义属性名
			var propId = dissolvePropertyName == "_Dissolve" ? DissolveID : Shader.PropertyToID(dissolvePropertyName);
			if (!mat.HasProperty(propId))
			{
				Debug.LogWarning($"材质上不存在属性 {dissolvePropertyName}");
				return;
			}
			if (dissolveCoroutine != null) StopCoroutine(dissolveCoroutine);
			if (duration <= 0f)
			{
				mat.SetFloat(propId, targetValue);
				return;
			}
			dissolveCoroutine = StartCoroutine(DissolveRoutine(mat, propId, targetValue, duration));
		}
		List<NoteData> Parse()
		{
			var noteDataList = new List<NoteData>();
			var lines = levelTextAsset.text.Split('\n');
			var notePattern = new Regex(@"\[([0-9]{2}:[0-9]{2}\.[0-9]{2})\]([A-Za-z0-9]+)");
			foreach (var line in lines)
			{
				var trimmedLine = line.Trim();
				if (string.IsNullOrEmpty(trimmedLine)) continue;
				var match = notePattern.Match(trimmedLine);
				if (match.Success)
				{
					var timeStr = match.Groups[1].Value;
					var keyStr = match.Groups[2].Value;
					if (TryParseTimeFormat(timeStr, out var time) && TryParseKeyCode(keyStr, out var key))
					{
						var noteData = new NoteData
						{
							time = time,
							key = key,
						};
						noteDataList.Add(noteData);
					}
					else
					{
						Debug.LogWarning($"Failed to parse note data: {trimmedLine}");
					}
				}
			}
			return noteDataList;
		}
		bool TryParseTimeFormat(string timeStr, out float totalSeconds)
		{
			totalSeconds = 0f;
			var parts = timeStr.Split(':');
			if (parts.Length != 2) return false;
			if (!int.TryParse(parts[0], out var minutes)) return false;
			if (!float.TryParse(parts[1], out var seconds)) return false;
			totalSeconds = minutes * 60f + seconds;
			return true;
		}
		bool TryParseKeyCode(string keyStr, out KeyCode key)
		{
			key = KeyCode.None;
			switch (keyStr.ToUpper())
			{
				case "Q":
					key = KeyCode.Q;
					return true;
				case "W":
					key = KeyCode.W;
					return true;
				case "A":
					key = KeyCode.A;
					return true;
				case "S":
					key = KeyCode.S;
					return true;
				case "Z":
					key = KeyCode.Z;
					return true;
				case "X":
					key = KeyCode.X;
					return true;
				case "1":
					key = KeyCode.Alpha1;
					return true;
				case "2":
					key = KeyCode.Alpha2;
					return true;
				case "3":
					key = KeyCode.Alpha3;
					return true;
				case "4":
					key = KeyCode.Alpha4;
					return true;
				default:
					return false;
			}
		}
		void ParseAndGenerateNotes()
		{
			var noteDataList = Parse();
			foreach (var noteData in noteDataList) GenerateNote(noteData);
		}
		void GenerateNote(NoteData noteData)
		{
			var distanceToTravel = noteData.time * Note3DModel.MOVE_SPEED;
			var localStartPosition = targetPosition + Vector3.right * distanceToTravel;
			var noteInstance = Instantiate(note3DPrefab, transform);
			noteInstance.gameObject.SetActive(true);
			noteInstance.transform.localPosition = localStartPosition;
			noteInstance.gameObject.name = $"Note_{noteData.key}";
			noteInstance.Initialize(noteData, transform);
			activeNotes.Add(noteInstance);
		}
		void DetectInput()
		{
			KeyCode[] keysToCheck =
				{ KeyCode.Q, KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.Z, KeyCode.X, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, };
			foreach (var key in keysToCheck)
				if (Input.GetKeyDown(key))
					ProcessInput(key);
		}
		/// <summary>
		///     订阅或取消订阅来自 Player 的输入事件（腿部动作/情绪触发）。
		/// </summary>
		/// <param name="subscribe">true=订阅; false=取消订阅</param>
		void SubscribePlayerInputEvents(bool subscribe)
		{
			if (player == null) return;
			var hand = player.HandIkInput;
			if (hand == null) return;
			if (subscribe)
			{
				hand.OnLeftLegChanged += OnLeftLegChanged;
				hand.OnRightLegChanged += OnRightLegChanged;
				player.OnEmotionTriggered += OnEmotionTriggered;
			}
			else
			{
				hand.OnLeftLegChanged -= OnLeftLegChanged;
				hand.OnRightLegChanged -= OnRightLegChanged;
				player.OnEmotionTriggered -= OnEmotionTriggered;
			}
		}
		void OnLeftLegChanged() =>
			// 将当前左腿姿态映射为对应按键
			MapLegPoseToKey(true, player.HandIkInput.LeftLeg);
		void OnRightLegChanged() => MapLegPoseToKey(false, player.HandIkInput.RightLeg);
		void OnEmotionTriggered(Player.EmotionCode emotion)
		{
			var key = emotion switch
			{
				Player.EmotionCode.Hi => KeyCode.Alpha1,
				Player.EmotionCode.Surprise => KeyCode.Alpha2,
				Player.EmotionCode.Shy => KeyCode.Alpha3,
				Player.EmotionCode.Angry => KeyCode.Alpha4,
				_ => KeyCode.None,
			};
			if (key != KeyCode.None) ProcessInput(key);
		}
		void MapLegPoseToKey(bool isLeft, LegPoseCode pose)
		{
			var key = KeyCode.None;
			switch (pose)
			{
				case LegPoseCode.LiftForward:
					key = isLeft ? KeyCode.Q : KeyCode.W;
					break;
				case LegPoseCode.LiftUp:
					key = isLeft ? KeyCode.A : KeyCode.S;
					break;
				case LegPoseCode.LiftBackward:
					key = isLeft ? KeyCode.Z : KeyCode.X;
					break;
				// Idle 不触发
			}
			if (key != KeyCode.None) ProcessInput(key);
		}
		void ProcessInput(KeyCode inputKey)
		{
			Note3DModel targetNote = null;
			foreach (var note in activeNotes)
				if (note.noteData.key == inputKey && IsNoteInDetectionArea(note))
				{
					targetNote = note;
					break;
				}
			if (targetNote != null)
			{
				TriggerCorrectEvent(targetNote);
				RemoveNote(targetNote, true);
			}
			else
			{
				TriggerWrongEvent(inputKey);
			}
		}
		void RemoveNote(Note3DModel note, bool isHit)
		{
			activeNotes.Remove(note);
			note.DestroyNote(isHit);
			if (activeNotes.Count == 0) EndGame();
		}
		void TriggerCorrectEvent(Note3DModel note)
		{
			correctCount++;
			particle.Instantiate(particle.transform.parent).Play();
			Debug.Log($"Correct! Key: {note.noteData.key}, Score: {correctCount}");
		}
		void TriggerWrongEvent(KeyCode inputKey)
		{
			wrongCount++;
			Debug.Log($"Wrong! Key: {inputKey}, Wrong Count: {wrongCount}");
		}
		void TriggerMissEvent(Note3DModel note)
		{
			missCount++;
			Debug.Log($"Miss! Key: {note.noteData.key}, Miss Count: {missCount}");
		}
		void OnNoteEnterDetectionArea(Note3DModel note) => Debug.Log($"音符 {note.noteData.key} 进入检测区域");
		void OnNoteExitDetectionArea(Note3DModel note)
		{
			Debug.Log($"音符 {note.noteData.key} 离开检测区域");
			if (activeNotes.Contains(note))
			{
				TriggerMissEvent(note);
				RemoveNote(note, false);
			}
		}
		bool IsNoteInDetectionArea(Note3DModel note) => noteDetector != null && noteDetector.IsNoteInArea(note);
		void EndGame()
		{
			Debug.Log($"Game End! Correct: {correctCount}, Wrong: {wrongCount}, Miss: {missCount}");
			gameEndCallback?.Invoke((correctCount, wrongCount, missCount));
		}
		System.Collections.IEnumerator DissolveRoutine(Material mat, int propId, float target, float duration)
		{
			var start = mat.GetFloat(propId);
			var elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				var t = Mathf.Clamp01(elapsed / duration);
				mat.SetFloat(propId, Mathf.Lerp(start, target, t));
				yield return null;
			}
			mat.SetFloat(propId, target);
			dissolveCoroutine = null;
		}
		void HideAllHintsAtStart()
		{
			try
			{
#if UNITY_2023_1_OR_NEWER
				var root = FindFirstObjectByType<GameRoot>();
#else
				var root = UnityEngine.Object.FindObjectOfType<GameRoot>();
#endif
				if (root != null && root.UiCamera != null)
				{
					// 隐藏所有 UI 提示（包含 Q/W/A/S/Z/X 以及 1/2/3/4 等）
					var dict = root.UiCamera.Hints;
					if (dict != null)
						foreach (var kv in dict)
							if (kv.Value != null)
								kv.Value.Hide();
				}
				// 隐藏场景中的背景提示（如果有）
#if UNITY_2023_1_OR_NEWER
				var bgHints = FindObjectsByType<BackgroundHint>(FindObjectsSortMode.None);
#else
				var bgHints = UnityEngine.Object.FindObjectsOfType<BackgroundHint>();
#endif
				foreach (var h in bgHints)
					if (h != null && h.isActiveAndEnabled)
						h.Hide();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
