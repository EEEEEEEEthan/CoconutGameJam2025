using System;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Gameplay.DanceGame
{
	public class NoteDetector : MonoBehaviour
	{
		public static Action<Note3DModel> OnNoteEnter;
		public static Action<Note3DModel> OnNoteExit;
		[SerializeField] MeshRenderer meshRenderer;
		[SerializeField] Color highlightColor = Color.red;
		readonly List<Note3DModel> notesInArea = new();
		Color originalColor;
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
					UpdateColor();
					Debug.Log("Note destroyed, manually triggered exit logic", this);
				}
		}
		void OnTriggerEnter(Collider other)
		{
			var note = other.GetComponentInParent<Note3DModel>();
			if (note != null && !notesInArea.Contains(note))
			{
				notesInArea.Add(note);
				meshRenderer.sharedMaterial.color = highlightColor;
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
				if (notesInArea.Count == 0) meshRenderer.sharedMaterial.color = originalColor;
				OnNoteExit?.Invoke(note);
				Debug.Log($"音符 {note.name} 离开检测区域", this);
			}
		}
		public bool IsNoteInArea(Note3DModel note) => notesInArea.Contains(note);
		void UpdateColor()
		{
			var targetColor = notesInArea.Count > 0 ? highlightColor : originalColor;
			meshRenderer.sharedMaterial.color = targetColor;
		}
	}
}
