using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	abstract class SmoothVector3Transformer : MonoBehaviour
	{
		Vector3 preferredValue;
		Vector3 velocity;
		float smoothTime;
		Action callback;
		protected abstract Vector3 Value { get; set; }
		void Update()
		{
			var value = Vector3.SmoothDamp(Value,
				preferredValue,
				ref velocity,
				smoothTime,
				float.MaxValue,
				UnityEngine.Time.unscaledDeltaTime);
			if (Vector3.Distance(value, preferredValue) < 0.001f)
			{
				Value = preferredValue;
				enabled = false;
				var callback = this.callback;
				this.callback = null;
				callback?.TryInvoke();
			}
			else
			{
				Value = value;
			}
		}
		void OnEnable()
		{
			preferredValue = Value;
			velocity = Vector3.zero;
		}
		internal void SetValue(Vector3 value, float smoothTime, Action callback)
		{
			if (smoothTime <= 0)
			{
				Value = value;
				enabled = false;
				this.callback = null;
				callback?.TryInvoke();
				return;
			}
			enabled = true;
			preferredValue = value;
			this.smoothTime = smoothTime;
			this.callback = callback;
		}
	}
}
