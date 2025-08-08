using Game.FingerRigging;
using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class Hand : MonoBehaviour
	{
		[SerializeField] Transform handRoot;
		[SerializeField] Finger left;
		[SerializeField] Finger right;
		public Transform HandRoot => handRoot;
		public Finger Left => left;
		public Finger Right => right;

	}
}
