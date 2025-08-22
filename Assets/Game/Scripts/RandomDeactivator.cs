using UnityEngine;
using Random = UnityEngine.Random;
namespace Game.Scripts
{
	public class RandomDeactivator : MonoBehaviour
	{
		void Awake()
		{
			if (Random.value < 0.5f) gameObject.SetActive(false);
		}
	}
}
