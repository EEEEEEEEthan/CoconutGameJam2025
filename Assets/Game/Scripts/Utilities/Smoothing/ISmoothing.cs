using System;
using UnityEngine;
namespace Game.Utilities.Smoothing
{
	public interface ISmoothing
	{
		public float Value { get; }
		event Action<float> OnValueChanged;
		void Set(float value, float smoothTime);
	}
	public interface ISmoothing3
	{
		public Vector3 Value { get; }
		event Action<Vector3> OnValueChanged;
		void Set(Vector3 value, float smoothTime);
	}
}
