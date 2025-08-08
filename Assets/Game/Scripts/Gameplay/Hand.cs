using Game.FingerRigging;
using UnityEngine;
namespace Game.Gameplay
{
	public class Hand : MonoBehaviour
	{
		[SerializeField] Transform handRoot;
		[SerializeField] Finger left;
		[SerializeField] Finger right;
		[SerializeField] RaycastSource raycastSource;
		public Transform HandRoot => handRoot;
		public Finger Left => left;
		public Finger Right => right;
		public RaycastSource RaycastSource => raycastSource;
	}
}
