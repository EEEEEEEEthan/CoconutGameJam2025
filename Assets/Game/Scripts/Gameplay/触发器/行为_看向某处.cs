using System;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Gameplay.触发器
{
	public class 行为_看向某处 : GameBehaviour
	{
		[SerializeField] Transform lookTarget;
		[SerializeField] float keepSeconds = 2f;
		[SerializeField] float fov = 12f;
		void Awake() => enabled = false;
		async void OnEnable()
		{
			try
			{
				GameRoot.CameraController.LookAt(lookTarget, fov);
				await MainThreadTimerManager.Await(keepSeconds);
				GameRoot.CameraController.LookAtPlayer();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
