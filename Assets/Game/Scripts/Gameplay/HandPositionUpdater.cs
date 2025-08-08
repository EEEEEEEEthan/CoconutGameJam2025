using Game.Utilities;
using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class HandPositionUpdater : MonoBehaviour
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
			var position = (hand.Left.Target.position + hand.Right.Target.position) * 0.5f + offset;
			hand.HandRoot.position = position.WithY(Mathf.Min(hand.Left.Tip.position.y, hand.Right.Tip.position.y) + YOffset + offset.y);
		}
	}
}
