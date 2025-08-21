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

        Transform managerTransform;
        float gameStartTime;
        bool isDestroyed = false;
        void Awake()
        {
            meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
        }
        void Update()
		{
			if (isDestroyed) return;
			var currentGameTime = Time.time - gameStartTime;
			var timeRemaining = noteData.time - currentGameTime;
			var targetX = timeRemaining * MOVE_SPEED;
			var currentPosition = transform.localPosition;
			currentPosition.x = targetX;
			transform.localPosition = currentPosition;
		}
        public void Initialize(NoteData data, Transform manager)
		{
			noteData = data;
			managerTransform = manager;
			gameStartTime = Time.time;
			var texture = noteData.key switch
			{
				KeyCode.A => ResourceTable.aPng1.Main,
				KeyCode.S => ResourceTable.sPng1.Main,
				KeyCode.W => ResourceTable.wPng1.Main,
				KeyCode.Q => ResourceTable.qPng1.Main,
				KeyCode.X => ResourceTable.xPng1.Main,
				KeyCode.Z => ResourceTable.zPng1.Main,
				KeyCode.Alpha1 => ResourceTable.hiPng1.Main,
				KeyCode.Alpha2 => ResourceTable.surprisePng1.Main,
				KeyCode.Alpha3 => ResourceTable.shyPng1.Main,
				KeyCode.Alpha4 => ResourceTable.angryPng1.Main,
				_ => null,
			};
			if (texture != null)
			{
				meshRenderer.sharedMaterial.mainTexture = texture;
			}
		}
		public void DestroyNote(bool isHit)
		{
			isDestroyed = true;
			Destroy(gameObject);
		}
	}
}
