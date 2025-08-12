using System;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Utilities.Smoothing
{
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
