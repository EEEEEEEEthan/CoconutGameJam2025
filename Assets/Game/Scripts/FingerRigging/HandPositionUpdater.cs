using ReferenceHelper;
using UnityEngine;
namespace Game.FingerRigging
{
	class HandPositionUpdater : MonoBehaviour
	{
		[SerializeField, ObjectReference,] Hand hand;
		[SerializeField] bool groundFix = true;
		[SerializeField] float gravity;
		[SerializeField] Vector3 offset;
		[SerializeField, HideInInspector,] Vector3 preferredPosition;
		[SerializeField, HideInInspector,] Vector3 smoothVelocity;
		[SerializeField, HideInInspector,] float jumpVelocity;
		[SerializeField, HideInInspector,] bool crunching;
		[SerializeField, HideInInspector,] bool jumping;
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
			preferredPosition.y = Mathf.Min(hand.Left.Tip.position.y, hand.Right.Tip.position.y) + offset.y;
			if (groundFix && jumpVelocity <= 0)
			{
				var backupPos = hand.HandRoot.position;
				hand.HandRoot.position = preferredPosition;
				float groundFix;
				if (LeftOffset.HasValue && RightOffset.HasValue)
					groundFix = Mathf.Max(LeftOffset.Value, RightOffset.Value);
				else if (LeftOffset.HasValue)
					groundFix = LeftOffset.Value;
				else if (RightOffset.HasValue)
					groundFix = RightOffset.Value;
				else
					groundFix = 0;
				hand.HandRoot.position = backupPos;
				preferredPosition += new Vector3(0, groundFix, 0);
			}
			if (jumping)
			{
				if (jumpVelocity > 0)
				{
					jumpVelocity -= Time.deltaTime * Mathf.Abs(gravity);
					preferredPosition.y += jumpVelocity * Time.deltaTime;
				}
				else if (hand.RaycastSource.CenterHitPoint.HasValue)
				{
					var pos = hand.RaycastSource.CenterHitPoint.Value;
					var handPos = hand.HandRoot.position;
					if (handPos.y > pos.y && handPos.y - pos.y > 0.01f)
					{
						jumpVelocity -= Time.deltaTime * Mathf.Abs(gravity);
						preferredPosition.y += jumpVelocity * Time.deltaTime * 2;
					}
					else if (jumpVelocity <= 0)
					{
						jumping = false;
						jumpVelocity = 0;
					}
				}
			}
			var truePreferred = crunching ? preferredPosition + Vector3.down * 0.01f : preferredPosition;
			hand.HandRoot.position = Vector3.SmoothDamp(hand.HandRoot.position, truePreferred, ref smoothVelocity, 0.1f);
		}
		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(preferredPosition, 0.02f);
		}
		public void Jump(float speed)
		{
			if (jumping)
			{
				Debug.LogError("Already jumping, cannot jump again!");
				return;
			}
			jumping = true;
			jumpVelocity = speed;
		}
		public void Crunch(bool crunch) => crunching = crunch;
	}
}
