using System;
using System.Collections;
using Game.FingerRigging;
using UnityEngine;
namespace Game.Gameplay
{
	public class LegSmoothing : MonoBehaviour
	{
		[SerializeField] float speed = 1;
		[SerializeField] Finger otherFinger;
		[SerializeField] Vector3 preferredPosition;
		[SerializeField] Vector3 velocity;
		void Awake()
		{
			Step(transform.position, 0.01f);
		}
		void Update() => transform.position = Vector3.SmoothDamp(transform.position, preferredPosition, ref velocity, 0.1f);
		public void Step(Vector3 target, float height)
		{
			StopAllCoroutines();
			StartCoroutine(SetStep(target, height));
		}
		/// <param name="target">目标坐标</param>
		/// <param name="height">最高高度</param>
		IEnumerator SetStep(Vector3 target, float height)
		{
			// 倒抛物线 从preferredPosition开始,到target结束,最高点y = otherFinger.Tip.position.y + height
			// 携程更新 preferredPosition
			var startPos = preferredPosition;
			var endPos = target;
			var maxHeight = otherFinger.Tip.position.y + height;

			// 计算抛物线的持续时间，基于距离
			var distance = Vector3.Distance(startPos, endPos);
			var duration = distance / (speed * 2f); // 调整速度系数
			var elapsedTime = 0f;
			while (elapsedTime < duration)
			{
				// 计算时间插值 (0 到 1)
				var t = elapsedTime / duration;

				// 水平插值 (X和Z轴)
				var x = Mathf.Lerp(startPos.x, endPos.x, t);
				var z = Mathf.Lerp(startPos.z, endPos.z, t);

				// 抛物线高度计算 (倒抛物线: y = -4h*t*(t-1) + startY)
				// 当t=0时y=startY, 当t=0.5时y=maxHeight, 当t=1时y=endY
				var heightOffset = maxHeight - Mathf.Max(startPos.y, endPos.y);
				var y = Mathf.Lerp(startPos.y, endPos.y, t) + heightOffset * 4f * t * (1f - t);

				// 更新preferredPosition
				preferredPosition = new(x, y, z);
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			// 确保最终位置准确
			preferredPosition = endPos;
		}
	}
}
