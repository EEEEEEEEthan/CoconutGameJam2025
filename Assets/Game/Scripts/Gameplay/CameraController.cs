using Game.Utilities.Transformers;
using UnityEngine;
namespace Game.Gameplay
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] Transform target;
		[SerializeField] float velocityMultiplier = 1f;
		Vector3 preferredPosition;
		Vector3? lastTarget;
		void Update()
		{
			if (!target) return;
			if (lastTarget.HasValue && target.position == lastTarget) return;
			var velocity = lastTarget.HasValue ? (target.position - lastTarget.Value) / Time.unscaledDeltaTime : Vector3.zero;
			lastTarget = target.position;
			preferredPosition = target.position + velocity * velocityMultiplier;
			var localPosition = transform.parent ? transform.parent.InverseTransformPoint(preferredPosition) : preferredPosition;
			transform.SmoothMoveTo(localPosition, 0.1f);
		}
		void OnDisable() => lastTarget = null;
	}
}
