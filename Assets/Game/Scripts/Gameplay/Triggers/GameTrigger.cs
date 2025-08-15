using System;
using Game.ResourceManagement;
using UnityEngine;
using UnityEngine.Events;
namespace Game.Gameplay.Triggers
{
	public abstract class GameTrigger : GameBehaviour
	{
		protected static GameObject Create(Transform parent)
		{
			// default cube
			var triggerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			triggerObject.name = "Trigger";
			triggerObject.layer = (int)LayerCode.Detector;
			triggerObject.transform.SetParent(parent, false);
			triggerObject.transform.localPosition = Vector3.zero;
			triggerObject.transform.localRotation = Quaternion.identity;
			triggerObject.transform.localScale = Vector3.one * 0.1f;
			var collider = triggerObject.GetComponent<Collider>();
			collider.isTrigger = true;
			return triggerObject;
		}
		[SerializeField] public UnityEvent callback;
		[SerializeField] int maxTriggerCount = int.MaxValue;
		[SerializeField] float coolDownSeconds = 0.5f;
		Action<Player.EmotionCode> onEmotionTriggered;
		float lastTriggerTime;
		protected virtual void Awake()
		{
			if (TryGetComponent<MeshRenderer>(out var renderer)) renderer.enabled = false;
		}
		protected void TryTrigger()
		{
			if (--maxTriggerCount < 0) return;
			if (Time.time - lastTriggerTime < coolDownSeconds) return;
			lastTriggerTime = Time.time;
			Debug.Log($"{name} triggered!", this);
#if UNITY_EDITOR
			// ping this
			UnityEditor.EditorGUIUtility.PingObject(this);
#endif
			try
			{
				callback?.Invoke();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
