using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	abstract class LinearVector2Transformer : MonoBehaviour
	{
		Vector2 preferredValue;
		float speed;
		Action callback;
		protected abstract Vector2 Value { get; set; }
		void Update()
		{
			var value = Vector2.MoveTowards(Value, preferredValue, speed * UnityEngine.Time.unscaledDeltaTime);
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
		void OnEnable() => preferredValue = Value;
		internal void SetValue(Vector2 value, float duration, Action callback)
		{
			if (duration <= 0)
			{
				preferredValue = Value = value;
				enabled = false;
				this.callback = null;
				callback?.TryInvoke();
				return;
			}
			enabled = true;
			preferredValue = value;
			speed = Vector2.Distance(Value, preferredValue) / duration;
			this.callback = callback;
		}
	}
}
