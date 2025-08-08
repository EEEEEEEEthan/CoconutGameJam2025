using Game.FingerRigging;
using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class HandRootPositionUpdater : MonoBehaviour
	{
		[SerializeField] Transform handRoot;
		[SerializeField] Finger left;
		[SerializeField] Finger right;
		[SerializeField] Vector3 offset;
		public float YOffset
		{
			get => offset.y;
			set => offset.y = value;
		}
		void LateUpdate() => handRoot.position = (left.Target.position + right.Target.position) * 0.5f + offset;
	}
}
