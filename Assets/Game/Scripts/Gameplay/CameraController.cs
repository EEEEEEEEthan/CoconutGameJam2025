using Game.Utilities;
using Game.Utilities.Smoothing;
using ReferenceHelper;
using Unity.Cinemachine;
using UnityEngine;
namespace Game.Gameplay
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField, ObjectReference("Main Camera"),]
		Camera mainCamera;
		[SerializeField, ObjectReference("CinemachineCamera"),]
		CinemachineBasicMultiChannelPerlin linearMultiChannelPerlin;
		DampSmoothing shakeSmoothing;
		void Awake() => shakeSmoothing = new(0, v => linearMultiChannelPerlin.AmplitudeGain = v.Remapped(0, 1, 0, 0.3f));
		public void Shake(float duration)
		{
			shakeSmoothing.Set(1, 0);
			shakeSmoothing.Set(0, duration);
		}
	}
}
