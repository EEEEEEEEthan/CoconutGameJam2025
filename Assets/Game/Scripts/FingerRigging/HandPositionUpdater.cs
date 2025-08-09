using Game.Utilities;
using ReferenceHelper;
using UnityEngine;
namespace Game.FingerRigging
{
	class HandPositionUpdater : MonoBehaviour
	{
		[SerializeField, ObjectReference,] Hand hand;
		[SerializeField] Vector3 offset;
		[SerializeField, HideInInspector,] float yOffset;
		public float YOffset
		{
			get => yOffset;
			set => yOffset = value;
		}
		void LateUpdate()
		{
			hand.HandRoot.position =
				((hand.Left.Target.position + hand.Right.Target.position) * 0.5f + offset).WithY(
					Mathf.Min(hand.Left.Tip.position.y, hand.Right.Tip.position.y) + YOffset + offset.y);
			if(hand.HandYRoot)
				hand.HandYRoot.position = hand.HandRoot.position + new Vector3(0, YOffset, 0);
		}
	}
}
