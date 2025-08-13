using Game.Utilities.Smoothing;
using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay
{
	public class Sunlight : MonoBehaviour
	{
		[SerializeField, ObjectReference,] Light sun;
		readonly DampSmoothing damp;
		float defaultIntensity;
		Sunlight() => damp = new(0, v => sun.intensity = v);
		void Awake() => damp.Set(defaultIntensity = sun.intensity, 0);
		public void SetIntensity(float intensity, float duration)
		{
			if (sun is null) return;
			sun.intensity = intensity;
		}
		public void ResetIntensity(float duration) => SetIntensity(defaultIntensity, duration);
	}
}
