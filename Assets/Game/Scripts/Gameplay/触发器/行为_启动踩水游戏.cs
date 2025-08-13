using System;
using Game.FingerRigging;
using Game.Gameplay.WaterGame;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Gameplay
{
	public class 行为_启动踩水游戏1 : GameBehaviour
	{
		[SerializeField] Light lightBoy;
		[SerializeField] Light lightPlayer;
		[SerializeField] Animator boy;
		async void OnEnable()
		{
			while (enabled)
			{
				lightBoy.enabled = true;
				await MainThreadTimerManager.Await(2);
				boy.SetTrigger("A");
				await MainThreadTimerManager.Await(1);
				boy.SetTrigger("S");
				await MainThreadTimerManager.Await(1);
				lightBoy.enabled = false;
				lightPlayer.enabled = true;
				var result = await WaitForInputSequence(new[]
					{
						new ActionData
						{
							left = LegPoseCode.LiftUp,
							right = LegPoseCode.Idle,
						},
						new ActionData
						{
							left = LegPoseCode.Idle,
							right = LegPoseCode.LiftUp,
						},
					}
				);
			}
		}
		async void WaitForInputSequence(ActionData[] sequence, Action<bool> callback)
		{
			while (enabled) await MainThreadTimerManager.Await(0); // 等待1帧
		}
		Utilities.Awaitable<bool> WaitForInputSequence(ActionData[] sequence)
		{
			var awaitable = Utilities.Awaitable<bool>.Create(out var handle);
			void Callback(bool success) => handle.Set(success);
			WaitForInputSequence(sequence, Callback);
			return awaitable;
		}
	}
}
