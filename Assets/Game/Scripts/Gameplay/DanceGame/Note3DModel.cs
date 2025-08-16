using System;
using Game.ResourceManagement;
using UnityEngine;
namespace Game.Gameplay.DanceGame
{
    public class Note3DModel : MonoBehaviour
	{
        public const float MOVE_SPEED = 0.1f;
		public NoteData noteData;
        [SerializeField] MeshRenderer meshRenderer;
        public Action<Note3DModel> OnReachTarget;
        Transform managerTransform;
        float gameStartTime;
        void Awake()
        {
            meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
        }
        void Update()
		{
			var currentGameTime = Time.time - gameStartTime;
			var timeRemaining = noteData.time - currentGameTime;
			var targetX = timeRemaining * MOVE_SPEED;
			var currentPosition = transform.localPosition;
			currentPosition.x = targetX;
			transform.localPosition = currentPosition;
			if (timeRemaining <= 0f)
			{
				OnReachTarget?.Invoke(this);
				Destroy(gameObject);
			}
		}
        public void Initialize(NoteData data, Transform manager)
		{
			noteData = data;
			managerTransform = manager;
			gameStartTime = Time.time;
			var texture = noteData.key switch
			{
				KeyCode.A => ResourceTable.aPng.Main,
				KeyCode.S => ResourceTable.sPng.Main,
				KeyCode.W => ResourceTable.wPng.Main,
				KeyCode.Q => ResourceTable.qPng.Main,
				KeyCode.X => ResourceTable.xPng.Main,
				KeyCode.Z => ResourceTable.zPng.Main,
				_ => null,
			};
			if (texture != null)
			{
				meshRenderer.sharedMaterial.mainTexture = texture;
			}
		}
	}
}
