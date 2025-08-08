using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay
{
	public class LegSmoothing : MonoBehaviour
	{
		[SerializeField] float speed = 1;
		[SerializeField, HideInInspector,] Hermite.HermiteTimeline3 timeline;
		[SerializeField, HideInInspector,] Vector3 tangent;
		void Update()
		{
			var time = Time.time;
			timeline.Evaluate(time, out var p, out var v);
			transform.position = p;
			tangent = v;
			if (time > timeline.t1) enabled = false;
		}
		public void SetStep(Vector3 destination, Vector3 destinationTangent)
		{
			timeline = new()
			{
				p0 = transform.position,
				v0 = tangent,
				p1 = destination,
				v1 = destinationTangent,
				t0 = Time.time,
				t1 = Time.time + 1f / speed,
			};
			enabled = true;
		}
	}
}
