using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.DanceGame
{
    public class NoteDetector : MonoBehaviour
    {
        public static System.Action<Note3DModel> OnNoteEnter;
        public static System.Action<Note3DModel> OnNoteExit;
        [SerializeField] private MeshRenderer meshRenderer;
        private List<Note3DModel> notesInArea = new List<Note3DModel>();
        void Awake()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
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
        void Update()
        {
            for (int i = notesInArea.Count - 1; i >= 0; i--)
            {
                if (notesInArea[i] == null)
                {
                    notesInArea.RemoveAt(i);
                    UpdateColor();
                    Debug.Log("Note destroyed, manually triggered exit logic", this);
                }
            }
        }
        void OnTriggerExit(Collider other)
        {
            var note = other.GetComponentInParent<Note3DModel>();
            if (note != null && notesInArea.Contains(note))
            {
                notesInArea.Remove(note);
                if (notesInArea.Count == 0)
                {
                    SetColor(Color.white);
                }
                OnNoteExit?.Invoke(note);
                Debug.Log($"音符 {note.name} 离开检测区域", this);
            }
        }
        private void UpdateColor()
        {
            Color targetColor = notesInArea.Count > 0 ? Color.red : Color.white;
            SetColor(targetColor);
        }
        private void SetColor(Color color)
        {
            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                meshRenderer.sharedMaterial.color = color;
            }
        }
        public bool IsNoteInArea(Note3DModel note)
        {
            return notesInArea.Contains(note);
        }
        public List<Note3DModel> GetNotesInArea()
        {
            return new List<Note3DModel>(notesInArea);
        }
        public void CleanupDestroyedNotes()
        {
            notesInArea.RemoveAll(note => note == null);
            UpdateColor();
        }
    }
}
