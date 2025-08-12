using Game.ResourceManagement;
using Game.Utilities;
using Game.Utilities.UnityTools.Attributes;
using UnityEngine;
using UnityEngine.Events;
namespace Game.Gameplay.触发器
{
	[RequireComponent(typeof(BoxCollider))]
	public class 触发器_玩家进入 : MonoBehaviour
	{
#if UNITY_EDITOR
		[UnityEditor.CustomEditor(typeof(触发器_玩家进入))]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();
				var trigger = target as 触发器_玩家进入;
				var layer = (LayerCode)trigger.gameObject.layer;
				if (layer != LayerCode.Detector)
				{
					UnityEditor.EditorGUILayout.HelpBox($"请将触发器所在的 GameObject 的 Layer 设置为 {LayerCode.Detector}", UnityEditor.MessageType.Warning);
					if (GUILayout.Button($"设置为 {LayerCode.Detector}"))
					{
						trigger.gameObject.layer = (int)LayerCode.Detector;
						UnityEditor.EditorUtility.SetDirty(trigger.gameObject);
					}
				}
				var collider = trigger.GetComponent<BoxCollider>();
				if (collider.isTrigger == false)
				{
					UnityEditor.EditorGUILayout.HelpBox("请将 BoxCollider 的 Is Trigger 设置为 true", UnityEditor.MessageType.Warning);
					if (GUILayout.Button("设置为 true"))
					{
						collider.isTrigger = true;
						UnityEditor.EditorUtility.SetDirty(collider);
					}
				}
			}
		}
#endif
		[SerializeField, DisplayAs("一次性触发"),] bool autoDestroy;
		[SerializeField] UnityEvent onPlayerEnter;
		public void OnTriggerEnter(Collider other)
		{
			if (!enabled) return;
			if (other.GetComponentInParent<Player>())
			{
				onPlayerEnter?.Invoke();
				if (autoDestroy) GetComponent<BoxCollider>().Destroy();
			}
		}
#if UNITY_EDITOR
		GUIStyle style;
		void OnDrawGizmos()
		{
			Gizmos.color = new(0, 0, 0.6f);
			for (var i = onPlayerEnter.GetPersistentEventCount(); i-- > 0;)
			{
				var target = onPlayerEnter.GetPersistentTarget(i);
				if (target && target is GameObject targetObject)
				{
					var methodName = onPlayerEnter.GetPersistentMethodName(i);
					Gizmos.DrawLine(transform.position, targetObject.transform.position);
					Gizmos.DrawSphere(targetObject.transform.position, 0.01f);
					UnityEditor.Handles.Label(targetObject.transform.position,
						$"{target.name}(GameObject).{methodName}",
						style ??= new() { fontSize = 20, normal = new() { textColor = Color.blue, }, });
				}
				if (target && target is Component targetComponent)
				{
					var methodName = onPlayerEnter.GetPersistentMethodName(i);
					Gizmos.DrawLine(transform.position, targetComponent.transform.position);
					Gizmos.DrawSphere(targetComponent.transform.position, 0.01f);
					UnityEditor.Handles.Label(targetComponent.transform.position,
						$"{target.name}({targetComponent.GetType().Name}).{methodName}",
						style ??= new() { fontSize = 20, normal = new() { textColor = Color.blue, }, });
				}
			}
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(Vector3.zero, GetComponent<BoxCollider>().size);
		}
#endif
	}
}
