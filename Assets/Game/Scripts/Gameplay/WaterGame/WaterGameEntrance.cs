using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class WaterGameEntrance : GameBehaviour
	{
		[SerializeField, ObjectReference,] MeshRenderer meshRenderer;
		[SerializeField] Transform boy;
		void Awake() => meshRenderer.enabled = false;
		void OnCollisionEnter(Collision other)
		{
			if (other.collider.GetComponentInParent<Player>()) GameRoot.CameraController.LookAt(boy);
		}
#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			UnityEditor.Handles.Label(transform.position, nameof(WaterGameEntrance));
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, boy.transform.position);
		}
#endif
	}
}
