using System;
using Game.Utilities.UnityTools;
using UnityEngine;
using UnityEngine.Events;
namespace Game.Gameplay.触发器
{
	public class 行为_延迟 : GameBehaviour
	{
		[SerializeField] float seconds;
		[SerializeField] UnityEvent onDelayComplete;
		async void OnEnable()
		{
			try
			{
				await MainThreadTimerManager.Await(seconds);
				onDelayComplete.Invoke();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
