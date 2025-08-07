using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	public abstract class HermiteSingleTransformer : MonoBehaviour
	{
		CanvasGroup canvasGroup;
		Hermite.HermiteTimeline1 hermite;
		float velocity;
		Action callback;
		protected abstract float Value { get; set; }
		void Update()
		{
			hermite.Evaluate(UnityEngine.Time.unscaledTime + UnityEngine.Time.unscaledDeltaTime, out var value, out velocity);
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
				v0 = 0,
				t0 = UnityEngine.Time.time,
				p1 = value,
				v1 = 0,
				t1 = UnityEngine.Time.time + UnityEngine.Time.deltaTime,
			};
		}
		public void SetValue(float value, float smoothTime, Action callback)
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
				t0 = UnityEngine.Time.unscaledTime,
				p1 = value,
				v1 = 0,
				t1 = UnityEngine.Time.unscaledTime + smoothTime,
			};
			this.callback = callback;
		}
		public Awaitable SetValue(float value, float smoothTime)
		{
			var awaitable = Awaitable.Create(out var handle);
			SetValue(value, smoothTime, handle.Set);
			return awaitable;
		}
	}
}
