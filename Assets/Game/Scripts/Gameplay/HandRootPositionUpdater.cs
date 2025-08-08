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
		void Update()
		{
			var maxHeight = Mathf.Min(left.TipDistance, right.TipDistance);
			var sideLength = Mathf.Min(left.TipDistance, right.TipDistance);
			var width = (left.Tip.position - right.Tip.position).magnitude;
			var halfWidth = width * 0.5f;
			var height = Mathf.Sqrt(sideLength * sideLength - halfWidth * halfWidth);
			var down = Vector3.down * (maxHeight - height);
			handRoot.position = (left.Target.position + right.Target.position) * 0.5f + offset;
		}
	}
}
