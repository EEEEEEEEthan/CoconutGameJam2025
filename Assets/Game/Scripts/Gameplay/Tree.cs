using System;
using System.Collections;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Gameplay
{
	public class Tree : GameBehaviour
	{
		[SerializeField] float greetRadius = 0.2f; // 范围
		[Header("Shake Settings")] 
		[SerializeField] float shakeDuration = 1; // 持续时间
		[SerializeField] float shakeAngle = 3f; // 最大角度 ±2°
		[SerializeField] int shakeCycles = 8; // 震动循环次数（越大越快）

		Coroutine shakeRoutine;

		void OnEnable()
		{
			if (GameRoot && GameRoot.Player != null) GameRoot.Player.OnEmotionTriggered += OnPlayerEmotion;
		}
		void OnDisable()
		{
			if (GameRoot && GameRoot.Player != null) GameRoot.Player.OnEmotionTriggered -= OnPlayerEmotion;
			if (shakeRoutine != null)
			{
				StopCoroutine(shakeRoutine);
				shakeRoutine = null;
			}
		}
		async void OnPlayerEmotion(Player.EmotionCode emotion)
		{
			try
			{
				await MainThreadTimerManager.Await(1);
				// 开始树的摇晃动画
				if (shakeRoutine != null) StopCoroutine(shakeRoutine);
				shakeRoutine = StartCoroutine(Shake());
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		IEnumerator Shake()
		{
			float duration = Mathf.Max(0.01f, shakeDuration);
			float maxAngle = Mathf.Abs(shakeAngle);
			int cycles = Mathf.Max(1, shakeCycles);
			float elapsed = 0f;
			Quaternion startRot = transform.localRotation;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float p = Mathf.Clamp01(elapsed / duration);
				// 衰减 (1-p)，正弦往返震动
				float angle = maxAngle * (1f - p) * Mathf.Sin(p * cycles * Mathf.PI * 2f);
				Vector3 e = startRot.eulerAngles;
				e.z = startRot.eulerAngles.z + angle;
				transform.localRotation = Quaternion.Euler(e);
				yield return null;
			}
			transform.localRotation = startRot; // 复位
			shakeRoutine = null;
		}
	}
}
