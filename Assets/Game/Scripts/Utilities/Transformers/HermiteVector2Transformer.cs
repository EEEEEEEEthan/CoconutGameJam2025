using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	public abstract class HermiteVector2Transformer : MonoBehaviour
	{
		CanvasGroup canvasGroup;
		Hermite.HermiteTimeline2 hermite;
		Vector2 velocity;
		Action callback;
		protected abstract Vector2 Value { get; set; }
		void Update()
		{
			hermite.Evaluate(UnityEngine.Time.unscaledTime + UnityEngine.Time.unscaledDeltaTime,
				out var value,
				out velocity);
			Value = value;
			if (UnityEngine.Time.unscaledTime > hermite.t1)
			{
				enabled = false;
				var callback = this.callback;
				this.callback = null;
				callback?.TryInvoke();
			}
		}
		void OnEnable()
		{
			var value = Value;
			hermite = new()
			{
				p0 = value,
				v0 = Vector2.zero,
				t0 = UnityEngine.Time.unscaledTime,
				p1 = value,
				v1 = Vector2.zero,
				t1 = UnityEngine.Time.unscaledTime,
			};
		}
		internal void SetValue(Vector2 value, float smoothTime, Action callback)
		{
			if (smoothTime <= 0)
			{
				Value = value;
				enabled = false;
				return;
			}
			enabled = true;
			hermite = new()
			{
				p0 = Value,
				v0 = velocity,
				t0 = UnityEngine.Time.time,
				p1 = value,
				v1 = Vector2.zero,
				t1 = UnityEngine.Time.time + smoothTime,
			};
			this.callback = callback;
		}
	}
}
