using System;
using Game.ResourceManagement;
using Game.Utilities;
using ReferenceHelper;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
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
		Action tempOnLanded;
		public bool Jumping => jumping;
		public bool Crunching => crunching;
		public event Action OnLanded;
		void Awake() => jumping = false;
		void LateUpdate()
		{
			if (hand.Input.Weight < 1) return;
			if (!jumping)
			{
				preferredPosition = (hand.Left.Target.position + hand.Right.Target.position) * 0.5f + offset;
				preferredPosition.y = Mathf.Min(hand.Left.Target.position.y, hand.Right.Target.position.y) + offset.y;
				var minDistance = float.MaxValue;
				var legLength = 0f;
				var leftDistance = Vector3.Distance(hand.Left.Tip.position, hand.Left.Target.position);
				if(leftDistance < hand.Left.MaxDistance)
				{
					minDistance = leftDistance;
					legLength = hand.Left.MaxDistance;
				}
				var rightDistance = Vector3.Distance(hand.Right.Tip.position, hand.Right.Target.position);
				if (rightDistance < hand.Right.MaxDistance && rightDistance < minDistance)
				{
					minDistance = rightDistance;
					legLength = hand.Right.MaxDistance;
				}
				var height = Mathf.Sqrt(legLength * legLength - minDistance * minDistance);
				var down = legLength - height;
				preferredPosition += Vector3.down * (down * 15);  // 不知道为什么是15, 反正15效果最好
			}
			if (groundFix && jumpVelocity <= 0)
			{
				var backupPos = hand.HandRoot.position;
				hand.HandRoot.position = preferredPosition;
				float groundFix;
				var leftOffsetDetected = GetLeftOffset(out var leftOffset, out _);
				var rightOffsetDetected = GetRightOffset(out var rightOffset, out _);
				if (leftOffsetDetected && rightOffsetDetected)
					groundFix = Mathf.Max(leftOffset, rightOffset);
				else if (leftOffsetDetected)
					groundFix = leftOffset;
				else if (rightOffsetDetected)
					groundFix = rightOffset;
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
					if (handPos.y > pos.y && handPos.y - pos.y > 0.001f)
					{
						jumpVelocity -= Time.deltaTime * Mathf.Abs(gravity);
						preferredPosition.y += jumpVelocity * Time.deltaTime * 0.5f;
					}
					else if (jumpVelocity <= 0)
					{
						jumping = false;
						jumpVelocity = 0;
						var copy = tempOnLanded;
						tempOnLanded = null;
						copy?.TryInvoke();
						OnLanded?.TryInvoke();
					}
				}
			}
			var truePreferred = crunching ? preferredPosition + Vector3.down * 0.01f : preferredPosition;
			hand.HandRoot.position = Vector3.SmoothDamp(hand.HandRoot.position, truePreferred, ref smoothVelocity, 0.1f);
		}
		public void Jump(float speed, Action callback)
		{
			if (jumping)
			{
				Debug.LogError("Already jumping, cannot jump again!");
				return;
			}
			tempOnLanded = callback;
			jumping = true;
			jumpVelocity = speed;
		}
		public void Crunch(bool crunch) => crunching = crunch;
		bool GetLeftOffset(out float offset, out RaycastHit hit)
		{
			offset = 0;
			hit = default;
			if (hand.Left.Progress < 0.8f) return false;
			var ray = new Ray(hand.Left.Tip.position + Vector3.up, Vector3.down);
			if (!Physics.Raycast(ray, out hit, 10, (int)LayerMaskCode.Stand)) return false;
			offset = Mathf.Min(hit.point.y - hand.Left.Tip.position.y, 0);
			return true;
		}
		bool GetRightOffset(out float offset, out RaycastHit hit)
		{
			offset = 0;
			hit = default;
			if (hand.Right.Progress < 0.8f) return false;
			var ray = new Ray(hand.Right.Tip.position + Vector3.up, Vector3.down);
			if (!Physics.Raycast(ray, out hit, 10, (int)LayerMaskCode.Stand)) return false;
			offset = Mathf.Min(hit.point.y - hand.Right.Tip.position.y, 0);
			return true;
		}
	}
}
