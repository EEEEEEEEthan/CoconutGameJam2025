using System.Collections;
using UnityEngine;

namespace Game.Gameplay
{
	/// <summary>
	/// 背景提示：
	/// Enable 时：记录初始坐标 -> 立即放到 初始 + up * offsetHeight -> 1秒(可调) 内下降到初始位置。
	/// Hide(): 1秒(可调) 内从当前/初始位置上升到 初始 + up * offsetHeight。
	/// 位移曲线使用 AnimationCurve (0-1 输入, 0-1 输出)，可在 Inspector 中调节。
	/// </summary>
	public class BackgroundHint : MonoBehaviour
	{
		[Header("Animation Settings")] [SerializeField]
		private float offsetHeight = 1f; // 上升高度
		[SerializeField] private float duration = 1f; // 动画时长
		[SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 位移曲线

		private Vector3 _basePosition;
		private Coroutine _animRoutine;
		private bool _hasBasePosition;

		private void OnEnable()
		{
			// 记录初始坐标
			_basePosition = transform.position;
			_hasBasePosition = true;

			// 瞬移到 初始 + up * offsetHeight
			transform.position = _basePosition + Vector3.up * offsetHeight;

			// 开始下降动画
			StartAnim(_basePosition + Vector3.up * offsetHeight, _basePosition);
		}

		private void OnDisable()
		{
			StopAnim();
		}

		/// <summary>
		/// 外部调用：向上隐藏（升到 base + offsetHeight）。
		/// </summary>
		public void Hide()
		{
			if (!_hasBasePosition)
			{
				_basePosition = transform.position - Vector3.up * offsetHeight; // 尝试推断
				_hasBasePosition = true;
			}

			var target = _basePosition + Vector3.up * offsetHeight;
			StartAnim(transform.position, target);
		}

		private void StartAnim(Vector3 from, Vector3 to)
		{
			StopAnim();
			_animRoutine = StartCoroutine(AnimRoutine(from, to));
		}

		private void StopAnim()
		{
			if (_animRoutine != null)
			{
				StopCoroutine(_animRoutine);
				_animRoutine = null;
			}
		}

		private IEnumerator AnimRoutine(Vector3 from, Vector3 to)
		{
			if (duration <= 0f)
			{
				transform.position = to;
				yield break;
			}

			float t = 0f;
			while (t < duration)
			{
				float nt = t / duration; // 0-1
				float eval = curve != null ? curve.Evaluate(nt) : nt;
				transform.position = Vector3.LerpUnclamped(from, to, eval);
				yield return null;
				t += Time.deltaTime;
			}
			// 结束时对齐
			transform.position = to;
			_animRoutine = null;
		}
	}
}
