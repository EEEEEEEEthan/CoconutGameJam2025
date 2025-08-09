using ReferenceHelper;
using UnityEngine;
namespace Game.FingerRigging
{
	class HandPositionUpdater : MonoBehaviour
	{
		[SerializeField, ObjectReference,] Hand hand;
		[SerializeField] Vector3 offset;
		[SerializeField, HideInInspector,] float yOffset;
		[SerializeField, HideInInspector,] float velocity;
		public float YOffset
		{
			get => yOffset;
			set => yOffset = value;
		}
		void LateUpdate()
		{
			var position = (hand.Left.Tip.position + hand.Right.Tip.position) * 0.5f + offset;
			position.y = Mathf.Min(hand.Left.Tip.position.y, hand.Right.Tip.position.y) + YOffset + offset.y;
			//if (hand.HandYRoot) hand.HandYRoot.position = hand.HandRoot.position + new Vector3(0, YOffset, 0);
			if (!hand.LeftGroundDetect.Any && !hand.RightGroundDetect.Any)
			{
				velocity = Mathf.Lerp(velocity, 1, Time.deltaTime * 1f);
				position += +velocity * Time.deltaTime * Vector3.down;
			}
			else
			{
				velocity = 0;
			}
			hand.HandRoot.position = position;
		}
	}
}
