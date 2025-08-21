using System;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Gameplay
{
	public class Ball : GameBehaviour
	{
		[SerializeField] float greetRadius = 0.2f; // 范围
		[SerializeField] float upVelocity = 2f; // 向上速度
		Rigidbody _rb;
		void Awake() => _rb = GetComponent<Rigidbody>();
		void OnEnable()
		{
			if (GameRoot && GameRoot.Player != null) GameRoot.Player.OnEmotionTriggered += OnPlayerEmotion;
		}
		void OnDisable()
		{
			if (GameRoot && GameRoot.Player != null) GameRoot.Player.OnEmotionTriggered -= OnPlayerEmotion;
		}
		async void OnPlayerEmotion(Player.EmotionCode emotion)
		{
			try
			{
				await MainThreadTimerManager.Await(1);
				if (emotion != Player.EmotionCode.Hi) return; // 只响应打招呼
				if (!GameRoot || GameRoot.Player == null) return;
				// 距离检测
				if ((transform.position - GameRoot.Player.transform.position).sqrMagnitude <= greetRadius * greetRadius)
				{
					if (_rb == null) _rb = GetComponent<Rigidbody>();
					if (_rb != null) _rb.velocity = Vector3.up * upVelocity;
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
