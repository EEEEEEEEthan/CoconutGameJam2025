using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Gameplay.DanceGame
{
	public class DanceGameManager : MonoBehaviour
	{
		[SerializeField] Note3DModel note3DPrefab;
		[SerializeField] TextAsset levelTextAsset;
		[SerializeField] NoteDetector noteDetector;
		[SerializeField] DanceNPC danceNPC;
		[SerializeField] Rigidbody[] rigidbodies;
		[SerializeField] Transform[] unimportant;
		[SerializeField] float unimportantMoveSpeed = 0.01f; // units per second to move left
		[SerializeField] MeshRenderer backgroundMeshRenderer; // 背景网格渲染器
		[SerializeField] string dissolvePropertyName = "_Dissolve"; // 材质溶解属性名
		Coroutine dissolveCoroutine; // 当前溶解协程
		static readonly int DissolveID = Shader.PropertyToID("_Dissolve");
		readonly Vector3 targetPosition = Vector3.zero;
		readonly List<Note3DModel> activeNotes = new();
		Action<(int correct, int wrong, int miss)> gameEndCallback;
		int correctCount;
		int wrongCount;
		int missCount;
		float gameStartTime;
		async void Awake()
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
			DetectInput();
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
				GetComponent<AudioSource>().Play();
				noteDetector.gameObject.SetActive(true);
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
				foreach (var rigidobody in rigidbodies)
				{
					rigidobody.useGravity = false;
					await MainThreadTimerManager.Await(1);
				}
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
		}
		public void SetGameEndCallback(Action<(int correct, int wrong, int miss)> callback) => gameEndCallback = callback;
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
					if (TryParseTimeFormat(timeStr, out var time) && TryParseKeyCode(keyStr, out var keyCode))
					{
						var noteData = new NoteData
						{
							time = time,
							key = keyCode,
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
		bool TryParseKeyCode(string keyStr, out KeyCode keyCode)
		{
			keyCode = KeyCode.None;
			switch (keyStr.ToUpper())
			{
				case "Q":
					keyCode = KeyCode.Q;
					return true;
				case "W":
					keyCode = KeyCode.W;
					return true;
				case "A":
					keyCode = KeyCode.A;
					return true;
				case "S":
					keyCode = KeyCode.S;
					return true;
				case "Z":
					keyCode = KeyCode.Z;
					return true;
				case "X":
					keyCode = KeyCode.X;
					return true;
				case "1":
					keyCode = KeyCode.Alpha1;
					return true;
				case "2":
					keyCode = KeyCode.Alpha2;
					return true;
				case "3":
					keyCode = KeyCode.Alpha3;
					return true;
				case "4":
					keyCode = KeyCode.Alpha4;
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
		/// <summary>
		/// 平滑设置背景材质的 _Dissolve 参数。
		/// 如果 duration <= 0 则立即设置。
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
		System.Collections.IEnumerator DissolveRoutine(Material mat, int propId, float target, float duration)
		{
			float start = mat.GetFloat(propId);
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = Mathf.Clamp01(elapsed / duration);
				mat.SetFloat(propId, Mathf.Lerp(start, target, t));
				yield return null;
			}
			mat.SetFloat(propId, target);
			dissolveCoroutine = null;
		}
	}
}
