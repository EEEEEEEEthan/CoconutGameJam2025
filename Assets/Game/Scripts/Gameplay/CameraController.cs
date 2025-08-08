using UnityEngine;
namespace Game.Gameplay
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] Transform target;
		void Update()
		{
			//todo: 平滑
			if (target) transform.position = target.position;
		}
	}
}
