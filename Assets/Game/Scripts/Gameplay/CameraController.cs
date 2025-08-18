using Game.Gameplay.WaterGame;
using Game.Utilities;
using Game.Utilities.Smoothing;
using ReferenceHelper;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
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
		[SerializeField, ObjectReference("CinemachineCamera"),]
		VelocityCalculator velocityCalculator;
		readonly DampSmoothing fovSmoothing;
		DampSmoothing shakeSmoothing;
		float playerFov;
		public VelocityCalculator VelocityCalculator => velocityCalculator;
		CameraController() =>
			fovSmoothing = new(0,
				v =>
				{
					Assert.IsNotNull(cinemachineCamera);
					cinemachineCamera.Lens.FieldOfView = v;
				});
		void Awake()
		{
			playerFov = cinemachineCamera.Lens.FieldOfView;
			shakeSmoothing = new(0, v => linearMultiChannelPerlin.AmplitudeGain = v.Remapped(0, 1, 0, 0.3f));
			fovSmoothing.Set(cinemachineCamera.Lens.FieldOfView, 0);
			LookAtPlayer();
		}
		public void LookAtPlayer()
		{
			cinemachineCamera.Target.TrackingTarget = GameRoot.Player.CameraTarget;
			cinemachineFollow.FollowOffset = new(0, 0, -1);
			fovSmoothing.Set(playerFov, 0.5f);
		}
		public void LookAt(Transform target, float fov)
		{
			cinemachineCamera.Target.TrackingTarget = target;
			cinemachineFollow.FollowOffset = new(0, 0, -1);
			fovSmoothing.Set(fov, 0.5f);
		}
		public void Shake(float duration)
		{
			shakeSmoothing.Set(1, 0);
			shakeSmoothing.Set(0, duration);
		}
	}
}
