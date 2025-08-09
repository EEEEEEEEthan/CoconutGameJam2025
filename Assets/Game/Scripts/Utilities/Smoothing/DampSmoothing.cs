using System;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Utilities.Smoothing
{
	public interface ISmoothing
	{
		public float Value { get; }
		event Action<float> OnValueChanged;
		void Set(float value, float smoothTime);
	}
	public class LinearSmoothing : ISmoothing
	{
		int operationFlag;
		public float Value { get; private set; }
		public event Action<float> OnValueChanged;
		public LinearSmoothing(float initValue, Action<float> onValueChanged = null)
		{
			Value = initValue;
			OnValueChanged += onValueChanged;
		}
		public async void Set(float value, float smoothTime)
		{
			try
			{
				var flag = ++operationFlag;
				if (value == Value) return;
				if (smoothTime <= 0)
				{
					Value = value;
					OnValueChanged?.TryInvoke(Value);
					return;
				}
				var startTime = UnityEngine.Time.time;
				while (flag == operationFlag)
				{
					var elapsed = UnityEngine.Time.time - startTime;
					var progress = Mathf.Clamp01(elapsed / smoothTime);
					Value = Mathf.Lerp(Value, value, progress);
					if (progress >= 1)
					{
						Value = value;
						OnValueChanged?.TryInvoke(Value);
						break;
					}
					OnValueChanged?.TryInvoke(Value);
					await MainThreadTimerManager.Await(0);
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
	public class DampSmoothing : ISmoothing
	{
		float velocity;
		int operationFlag;
		public float Value { get; private set; }
		public event Action<float> OnValueChanged;
		public DampSmoothing(float initValue, Action<float> onValueChanged = null)
		{
			Value = initValue;
			velocity = 0f;
			OnValueChanged += onValueChanged;
		}
		public async void Set(float value, float smoothTime)
		{
			try
			{
				var flag = ++operationFlag;
				if (smoothTime <= 0)
				{
					Value = value;
					OnValueChanged?.TryInvoke(Value);
					velocity = 0f;
					return;
				}
				while (flag == operationFlag)
				{
					Value = Mathf.SmoothDamp(Value, value, ref velocity, smoothTime);
					if (Mathf.Approximately(Value, value))
					{
						Value = value;
						OnValueChanged?.TryInvoke(Value);
						break;
					}
					OnValueChanged?.TryInvoke(Value);
					await MainThreadTimerManager.Await(0);
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
