using ReferenceHelper;
using UnityEngine;
namespace Game.FingerRigging
{
	class HandPositionUpdater : MonoBehaviour
	{
		[SerializeField, ObjectReference,] Hand hand;
		[SerializeField] Vector3 offset;
		[SerializeField, HideInInspector,] float yOffset;
		[SerializeField, HideInInspector,] Vector3 preferredPosition;
		[SerializeField, HideInInspector,] Vector3 velocity;
		public float YOffset
		{
			get => yOffset;
			set => yOffset = value;
		}
		float LeftOffset
		{
			get
			{
				if (hand.Left.Progress < 0.9f) return 0;
				var hit = hand.Left.Tip.position.GetTerrainHit(1);
				if (hit.HasValue) return Mathf.Min(hit.Value.y - hand.Left.Tip.position.y, 0);
				return 0;
			}
		}
		float RightOffset
		{
			get
			{
				if (hand.Right.Progress < 0.9f) return 0;
				var hit = hand.Right.Tip.position.GetTerrainHit(1);
				if (hit.HasValue) return Mathf.Min(hit.Value.y - hand.Right.Tip.position.y, 0);
				return 0;
			}
		}
		void LateUpdate()
		{
			var position = (hand.Left.Tip.position + hand.Right.Tip.position) * 0.5f + offset;
			position.y = Mathf.Min(hand.Left.Tip.position.y, hand.Right.Tip.position.y) + YOffset + offset.y;
			var backupPos = hand.HandRoot.position;
			hand.HandRoot.position = position;
			var groundFix = Mathf.Max(LeftOffset, RightOffset);
			hand.HandRoot.position = backupPos;
			preferredPosition = position + new Vector3(0, groundFix, 0);
			if (hand.HandYRoot) hand.HandYRoot.position = hand.HandRoot.position + new Vector3(0, YOffset, 0);
			hand.HandRoot.position = Vector3.SmoothDamp(hand.HandRoot.position, preferredPosition, ref velocity, 0.1f);
		}
	}
}
