using Game.ResourceManagement;
using Game.Utilities.UnityTools.Attributes;
using UnityEngine;
using UnityEngine.Events;
namespace Game.Gameplay.触发器
{
	[RequireComponent(typeof(BoxCollider))]
	public class 触发器 : GameBehaviour
	{
#if UNITY_EDITOR
		[UnityEditor.CustomEditor(typeof(触发器), true)]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();
				var trigger = target as 触发器;
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
		[SerializeField] float coldDown = 0.5f;
		[SerializeField] UnityEvent actions;
		float lastTriggerTime;
		protected void Trigger()
		{
			if (!enabled) return;
			if (lastTriggerTime + coldDown > Time.time) return; // 冷却中
			lastTriggerTime = Time.time;
			actions?.Invoke();
			#if UNITY_EDITOR
			Debug.Log($"触发器 {name} 被触发个事件。", this);
			UnityEditor.EditorGUIUtility.PingObject(this);
			#endif
			if (autoDestroy) Destroy(this);
		}
#if UNITY_EDITOR
		GUIStyle styleCorrect;
		GUIStyle stileIncorrect;
		void OnDrawGizmos()
		{
			Gizmos.color = new(0, 0, 0.6f);
			for (var i = actions.GetPersistentEventCount(); i-- > 0;)
			{
				var target = actions.GetPersistentTarget(i);
				if (!target) continue;
				Transform targetTransform;
				switch (target)
				{
					case GameObject targetObject:
						targetTransform = targetObject.transform;
						break;
					case Component targetComponent:
						targetTransform = targetComponent.transform;
						break;
					default:
						continue;
				}
				var methodName = actions.GetPersistentMethodName(i);
				Gizmos.DrawLine(transform.position, targetTransform.position);
				Gizmos.DrawSphere(targetTransform.position, 0.01f);
				UnityEditor.Handles.Label(targetTransform.position,
					$"{target.name}({target.GetType().Name}).{methodName}",
					styleCorrect ??= new() { fontSize = 20, normal = new() { textColor = Color.blue, }, });
			}
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, GetComponent<BoxCollider>().size);
		}
#endif
	}
}
