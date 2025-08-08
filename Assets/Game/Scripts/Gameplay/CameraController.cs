using Game.Utilities.Transformers;
using UnityEngine;
namespace Game.Gameplay
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] Transform target;
		[SerializeField] float velocityMultiplier = 1f;
		Vector3 preferredPosition;
		void Update()
		{
			if (target is null) return;
			if (target.position != preferredPosition) return;
			var velocity = (target.position - preferredPosition) / Time.unscaledDeltaTime;
			preferredPosition = target.position + velocity * velocityMultiplier;
			var localPosition = transform.InverseTransformPoint(preferredPosition);
			transform.SmoothMoveTo(localPosition, 0.1f);
		}
	}
}
