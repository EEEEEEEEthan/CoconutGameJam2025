using Game.Utilities.UnityTools;
using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class WaterGameEntrance : GameBehaviour
	{
		[SerializeField, ObjectReference,] MeshRenderer meshRenderer;
		[SerializeField] Transform lookTarget;
		[SerializeField, Range(10, 20),] float lookAtDistance = 12f;
		[SerializeField, Range(0, 60),] float keepSeconds;
		void Awake() => meshRenderer.enabled = false;
		void OnTriggerEnter(Collider other)
		{
			if (other.GetComponentInParent<Player>()) Trigger();
		}
#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			UnityEditor.Handles.Label(transform.position, nameof(WaterGameEntrance));
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, lookTarget.transform.position);
		}
#endif
		async void Trigger()
		{
			GameRoot.CameraController.LookAt(lookTarget, 12);
			await MainThreadTimerManager.Await(keepSeconds);
			GameRoot.CameraController.LookAtPlayer();
		}
	}
}
