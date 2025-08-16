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
		readonly List<Note3DModel> notesInArea = new();
		void Awake()
		{
			var col = GetComponent<Collider>();
			if (col != null) col.isTrigger = true;
			meshRenderer.sharedMaterial = new(meshRenderer.sharedMaterial);
		}
		void Update()
		{
			for (var i = notesInArea.Count - 1; i >= 0; i--)
				if (notesInArea[i] == null)
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
				SetColor(Color.red);
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
				if (notesInArea.Count == 0) SetColor(Color.white);
				OnNoteExit?.Invoke(note);
				Debug.Log($"音符 {note.name} 离开检测区域", this);
			}
		}
		public bool IsNoteInArea(Note3DModel note) => notesInArea.Contains(note);
		void UpdateColor()
		{
			var targetColor = notesInArea.Count > 0 ? Color.red : Color.white;
			SetColor(targetColor);
		}
		void SetColor(Color color) => meshRenderer.sharedMaterial.color = color;
	}
}
