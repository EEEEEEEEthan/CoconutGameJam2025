using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	abstract class SmoothVector2Transformer : MonoBehaviour
	{
		Vector2 preferredValue;
		Vector2 velocity;
		float smoothTime;
		Action callback;
		protected abstract Vector2 Value { get; set; }
		void Update()
		{
			var value = Vector2.SmoothDamp(Value,
				preferredValue,
				ref velocity,
				smoothTime,
				float.MaxValue,
				UnityEngine.Time.unscaledDeltaTime);
			if (Vector2.Distance(value, preferredValue) < 0.001f)
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
			velocity = Vector2.zero;
		}
		internal void SetValue(Vector2 value, float smoothTime, Action callback)
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
