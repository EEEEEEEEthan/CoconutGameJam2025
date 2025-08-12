using System;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Utilities.Smoothing
{
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
				var startValue = Value;
				while (flag == operationFlag)
				{
					var elapsed = UnityEngine.Time.time - startTime;
					var progress = Mathf.Clamp01(elapsed / smoothTime);
					Value = Mathf.Lerp(startValue, value, progress);
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
	public class LinearSmoothing3 : ISmoothing3
	{
		int operationFlag;
		public Vector3 Value { get; private set; }
		public event Action<Vector3> OnValueChanged;
		public LinearSmoothing3(Vector3 initValue, Action<Vector3> onValueChanged = null)
		{
			Value = initValue;
			OnValueChanged += onValueChanged;
		}
		public async void Set(Vector3 value, float smoothTime)
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
				var startValue = Value;
				while (flag == operationFlag)
				{
					var elapsed = UnityEngine.Time.time - startTime;
					var progress = Mathf.Clamp01(elapsed / smoothTime);
					Value = Vector3.Lerp(startValue, value, progress);
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
}
