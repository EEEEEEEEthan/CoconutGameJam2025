using ReferenceHelper;
using UnityEngine;
namespace Game.FingerRigging
{
	class Hand : MonoBehaviour
	{
		[SerializeField] Transform handRoot;
		[SerializeField] Transform handYRoot;
		[SerializeField] Finger left;
		[SerializeField] Finger right;
		[SerializeField] RaycastSource raycastSource;
		[SerializeField, ObjectReference,] HandPositionUpdater handPositionUpdater;
		[SerializeField] HandIKInput input;
		public HandIKInput Input => input;
		public Transform HandRoot => handRoot;
		public Transform HandYRoot => handYRoot;
		public Finger Left => left;
		public Finger Right => right;
		public RaycastSource RaycastSource => raycastSource;
		public HandPositionUpdater HandPositionUpdater => handPositionUpdater;
	}
}
