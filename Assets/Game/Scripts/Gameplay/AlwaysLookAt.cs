using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class AlwaysLookAt : MonoBehaviour
	{
		[SerializeField] Transform target;
		void Update() => transform.LookAt(target);
	}
}
