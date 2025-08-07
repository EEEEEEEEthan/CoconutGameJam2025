using UnityEngine;
namespace Game.Utilities.Transformers
{
	public abstract class SmoothSingleTransformer : MonoBehaviour
	{
		float preferredValue;
		float velocity;
		float smoothTime;
		protected abstract float Value { get; set; }
		void Update()
		{
			var value = Value;
			value = Mathf.SmoothDamp(value,
				preferredValue,
				ref velocity,
				smoothTime,
				float.MaxValue,
				UnityEngine.Time.unscaledDeltaTime);
			if (Mathf.Approximately(value, preferredValue))
			{
				Value = preferredValue;
				enabled = false;
			}
			else
			{
				Value = value;
			}
		}
		void OnEnable()
		{
			preferredValue = Value;
			velocity = 0;
		}
		public void SetValue(float value, float smoothTime)
		{
			if (smoothTime <= 0)
			{
				Value = value;
				enabled = false;
				return;
			}
			enabled = true;
			preferredValue = value;
			this.smoothTime = smoothTime;
		}
	}
}
