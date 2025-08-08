using Game.FingerRigging;
using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay
{
	public class Hand : MonoBehaviour
	{
		[SerializeField] Transform handRoot;
		[SerializeField] Transform handYRoot;
		[SerializeField] Finger left;
		[SerializeField] Finger right;
		[SerializeField] RaycastSource raycastSource;
		[SerializeField, ObjectReference] HandPositionUpdater handPositionUpdater;
		[SerializeField, ObjectReference] JumpSmoothing jumpSmoothing;
		public Transform HandRoot => handRoot;
		public Transform HandYRoot => handYRoot;
		public Finger Left => left;
		public Finger Right => right;
		public RaycastSource RaycastSource => raycastSource;
		public HandPositionUpdater HandPositionUpdater => handPositionUpdater;
		public JumpSmoothing JumpSmoothing => jumpSmoothing;
	}
}
