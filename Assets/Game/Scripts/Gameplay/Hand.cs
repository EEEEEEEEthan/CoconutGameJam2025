using Game.FingerRigging;
using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay
{
	public class Hand : MonoBehaviour
	{
		[SerializeField] Transform handRoot;
		[SerializeField] Finger left;
		[SerializeField] Finger right;
		[SerializeField] RaycastSource raycastSource;
		[SerializeField, ObjectReference] HandPositionUpdater handPositionUpdater;
		public Transform HandRoot => handRoot;
		public Finger Left => left;
		public Finger Right => right;
		public RaycastSource RaycastSource => raycastSource;
		public HandPositionUpdater HandPositionUpdater => handPositionUpdater;
	}
}
