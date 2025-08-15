using System;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay
{
	public static class Extensions
	{
		public class JumpBehaviour : MonoBehaviour
		{
			Vector3 startPosition;
			Vector3 targetPosition;
			float jumpHeight;
			float jumpDuration;
			Action onComplete;
			float elapsedTime;
			void Update()
			{
				elapsedTime += Time.deltaTime;
				var progress = elapsedTime / jumpDuration;
				if (progress >= 1f)
				{
					// 跳跃完成
					transform.position = targetPosition;
					enabled = false;
					onComplete?.Invoke();
					return;
				}

				// 计算抛物线位置
				var currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
				var heightOffset = jumpHeight * 4f * progress * (1f - progress); // 抛物线公式
				currentPos.y += heightOffset;
				transform.position = currentPos;
			}
			public void Jump(Vector3 target, float height, float duration, Action callback)
			{
				startPosition = transform.position;
				targetPosition = target;
				jumpHeight = height;
				jumpDuration = duration;
				onComplete = callback;
				elapsedTime = 0f;
				enabled = true;
			}
		}
		public static void Jump(this Transform @this, Vector3 target, float height, float duration, Action callback)
		{
			var jumpBehaviour = @this.GetOrAddComponent<JumpBehaviour>();
			jumpBehaviour.Jump(target, height, duration, callback);
		}
		public static Utilities.Awaitable AwaitJump(this Transform @this, Vector3 target, float height, float duration)
		{
			var awaitable = Utilities.Awaitable.Create(out var handle);
			@this.Jump(target, height, duration, handle.Set);
			return awaitable;
		}
		public static WaitUntil WaitJump(this Transform @this, Vector3 target, float height, float duration)
		{
			var finished = false;
			@this.Jump(target, height, duration, () => finished = true);
			return new(() => finished);
		}
	}
}
