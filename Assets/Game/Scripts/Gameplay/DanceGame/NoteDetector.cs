using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Gameplay.DanceGame
{
	public class NoteDetector : MonoBehaviour
	{
		public static Action<Note3DModel> OnNoteEnter;
		public static Action<Note3DModel> OnNoteExit;
		[SerializeField] MeshRenderer meshRenderer;
		[SerializeField] MeshRenderer boardPrefab;
		readonly List<Note3DModel> notesInArea = new();
		Color originalColor;
	Coroutine colorRoutine;
		void Awake()
		{
			var col = GetComponent<Collider>();
			if (col != null) col.isTrigger = true;
			meshRenderer.sharedMaterial = new(meshRenderer.sharedMaterial);
			originalColor = meshRenderer.sharedMaterial.color;
		}
		void Update()
		{
			for (var i = notesInArea.Count - 1; i >= 0; i--)
				if (!notesInArea[i])
				{
					notesInArea.RemoveAt(i);
					Debug.Log("Note destroyed, manually triggered exit logic", this);
				}
		}
		void OnTriggerEnter(Collider other)
		{
			var note = other.GetComponentInParent<Note3DModel>();
			if (note != null && !notesInArea.Contains(note))
			{
				notesInArea.Add(note);
				//meshRenderer.sharedMaterial.color = highlightColor;
				OnNoteEnter?.Invoke(note);
				Debug.Log($"音符 {note.name} 进入检测区域", this);
			}
		}
		void OnTriggerExit(Collider other)
		{
			var note = other.GetComponentInParent<Note3DModel>();
			if (note != null && notesInArea.Contains(note))
			{
				notesInArea.Remove(note);
				//if (notesInArea.Count == 0) meshRenderer.sharedMaterial.color = originalColor;
				OnNoteExit?.Invoke(note);
				Debug.Log($"音符 {note.name} 离开检测区域", this);
			}
		}
		public bool IsNoteInArea(Note3DModel note) => notesInArea.Contains(note);


		// 单一API：先立即变为指定颜色，再平滑过渡回初始颜色
		public void SmoothFromColorToOriginal(Color fromColor, float duration)
		{
			if (meshRenderer == null) return;

			if (colorRoutine != null)
			{
				StopCoroutine(colorRoutine);
				colorRoutine = null;
			}

			// 设置起始颜色并开始过渡
			meshRenderer.sharedMaterial.color = fromColor;
			if (duration <= 0f)
			{
				meshRenderer.sharedMaterial.color = originalColor;
				return;
			}
			colorRoutine = StartCoroutine(LerpToOriginalRoutine(fromColor, originalColor, duration));
		}

		IEnumerator LerpToOriginalRoutine(Color from, Color to, float duration)
		{
			var mat = meshRenderer != null ? meshRenderer.sharedMaterial : null;
			if (mat == null) yield break;
			float t = 0f;
			while (t < duration)
			{
				t += Time.deltaTime;
				float p = Mathf.Clamp01(t / duration);
				mat.color = Color.Lerp(from, to, p);
				yield return null;
			}
			mat.color = to;
			colorRoutine = null;
		}
	}
}
