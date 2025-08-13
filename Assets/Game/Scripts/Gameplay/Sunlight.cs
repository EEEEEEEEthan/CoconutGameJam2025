using Game.Utilities.Smoothing;
using ReferenceHelper;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition; // 添加HDRP命名空间

namespace Game.Gameplay
{
	public class Sunlight : MonoBehaviour
	{
		[SerializeField, ObjectReference,] HDAdditionalLightData hdLight;
		readonly DampSmoothing damp;
		float defaultIntensity;
		
		Sunlight() => damp = new(0, v => hdLight.intensity = v);
		
		void Awake()
		{
			damp.Set(defaultIntensity = hdLight.intensity, 0);
		}
		public void SetIntensity(float intensity, float duration)
		{
			damp.Set(intensity, duration);
		}
		
		public void ResetIntensity(float duration) => SetIntensity(defaultIntensity, duration);
	}
}
