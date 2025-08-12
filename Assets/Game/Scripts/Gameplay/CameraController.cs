using Game.Utilities;
using Game.Utilities.Smoothing;
using ReferenceHelper;
using Unity.Cinemachine;
using UnityEngine;
namespace Game.Gameplay
{
	public class CameraController : GameBehaviour
	{
		[SerializeField, ObjectReference("Main Camera"),]
		Camera mainCamera;
		[SerializeField, ObjectReference("CinemachineCamera"),]
		CinemachineCamera cinemachineCamera;
		[SerializeField, ObjectReference("CinemachineCamera"),]
		CinemachineBasicMultiChannelPerlin linearMultiChannelPerlin;
		[SerializeField, ObjectReference("CinemachineCamera"),]
		CinemachineFollow cinemachineFollow;
		DampSmoothing shakeSmoothing;
		void Awake() => shakeSmoothing = new(0, v => linearMultiChannelPerlin.AmplitudeGain = v.Remapped(0, 1, 0, 0.3f));
		public void LookAtPlayer()
		{
			cinemachineCamera.Target.TrackingTarget = GameRoot.Player.transform;
			cinemachineFollow.FollowOffset = new(0.05f, 0.1f, -1);
		}
		public void LookAt(Transform target)
		{
			cinemachineCamera.Target.TrackingTarget = target;
			cinemachineFollow.FollowOffset = new(0, 0.1f, -1);
			cinemachineCamera.Lens.FieldOfView = 10;
		}
		public void Shake(float duration)
		{
			shakeSmoothing.Set(1, 0);
			shakeSmoothing.Set(0, duration);
		}
	}
}
