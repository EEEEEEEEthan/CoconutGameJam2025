using System.Collections;
using UnityEngine;
namespace Game.FingerRigging
{
	class LegSmoothing : MonoBehaviour
	{
		[SerializeField] float speed = 1;
		[SerializeField] Finger otherFinger;
		[SerializeField] Vector3 preferredPosition;
		[SerializeField] Vector3 velocity;
		void Awake() => Step(transform.position, 0.01f);
		void Update() => transform.position = Vector3.SmoothDamp(transform.position, preferredPosition, ref velocity, 0.1f);
		public void Step(Vector3 target, float height)
		{
			StopAllCoroutines();
			StartCoroutine(SetStep(target, height));
		}
		/// <param name="target">Target position</param>
		/// <param name="height">Maximum height</param>
		IEnumerator SetStep(Vector3 target, float height)
		{
			var startPos = preferredPosition;
			var endPos = target;
			var maxHeight = otherFinger.Tip.position.y + height;
			var distance = Vector3.Distance(startPos, endPos);
			var duration = distance / (speed * 2f);
			var elapsedTime = 0f;
			while (elapsedTime < duration)
			{
				var t = elapsedTime / duration;
				var x = Mathf.Lerp(startPos.x, endPos.x, t);
				var z = Mathf.Lerp(startPos.z, endPos.z, t);
				var heightOffset = maxHeight - Mathf.Max(startPos.y, endPos.y);
				var y = Mathf.Lerp(startPos.y, endPos.y, t) + heightOffset * 4f * t * (1f - t);
				preferredPosition = new(x, y, z);
				elapsedTime += Time.deltaTime;
				yield return null;
			}
			preferredPosition = endPos;
		}
	}
}
