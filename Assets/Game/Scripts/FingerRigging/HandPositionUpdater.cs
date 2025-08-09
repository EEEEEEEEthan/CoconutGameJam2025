using System;
using ReferenceHelper;
using UnityEngine;
namespace Game.FingerRigging
{
	class HandPositionUpdater : MonoBehaviour
	{
		[SerializeField, ObjectReference,] Hand hand;
		[SerializeField] bool groundFix = true;
		[SerializeField] Vector3 offset;
		[SerializeField, HideInInspector,] float yOffset;
		[SerializeField, HideInInspector,] Vector3 preferredPosition;
		[SerializeField, HideInInspector,] Vector3 velocity;
		public float YOffset
		{
			get => yOffset;
			set => yOffset = value;
		}
		float? LeftOffset
		{
			get
			{
				if (hand.Left.Progress < 0.9f) return null;
				var hit = hand.Left.Tip.position.GetTerrainHit(1);
				if (hit.HasValue) return Mathf.Min(hit.Value.y - hand.Left.Tip.position.y, 0);
				return null;
			}
		}
		float? RightOffset
		{
			get
			{
				if (hand.Right.Progress < 0.9f) return null;
				var hit = hand.Right.Tip.position.GetTerrainHit(1);
				if (hit.HasValue) return Mathf.Min(hit.Value.y - hand.Right.Tip.position.y, 0);
				return null;
			}
		}
		void LateUpdate()
		{
			preferredPosition = (hand.Left.Tip.position + hand.Right.Tip.position) * 0.5f + offset;
			preferredPosition.y = Mathf.Min(hand.Left.Tip.position.y, hand.Right.Tip.position.y) + YOffset + offset.y;
			if (groundFix)
			{
				var backupPos = hand.HandRoot.position;
				hand.HandRoot.position = preferredPosition;
				float groundFix;
				if (LeftOffset.HasValue && RightOffset.HasValue) groundFix = Mathf.Max(LeftOffset.Value, RightOffset.Value);
				else if (LeftOffset.HasValue) groundFix = LeftOffset.Value;
				else if (RightOffset.HasValue) groundFix = RightOffset.Value;
				else
					groundFix = 0;
				hand.HandRoot.position = backupPos;
				preferredPosition += new Vector3(0, groundFix, 0);
			}
			if (hand.HandYRoot) hand.HandYRoot.position = hand.HandRoot.position + new Vector3(0, YOffset, 0);
			hand.HandRoot.position = Vector3.SmoothDamp(hand.HandRoot.position, preferredPosition, ref velocity, 0.05f);
		}
		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(preferredPosition, 0.02f);
		}
	}
}
