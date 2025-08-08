using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class Finger : MonoBehaviour
	{
		[SerializeField] FingerMuscle muscle;
		[SerializeField] Transform target;
		[SerializeField] Transform hint;
		void Update()
		{
			var distance = transform.position - target.position;
			muscle.Progress = distance.magnitude / muscle.MaxLength;
		}
	}
}
