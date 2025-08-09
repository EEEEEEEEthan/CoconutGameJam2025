using System;
using Game.Utilities;
using Game.Utilities.Smoothing;
using ReferenceHelper;
using UnityEngine;
namespace Game.FingerRigging
{
	public class HandIKInput : MonoBehaviour
	{
		[SerializeField, ObjectReference,] Hand hand;
		[SerializeField, ObjectReference,] HandPositionUpdater handPositionUpdater;
		[SerializeField] LegSmoothing leftLegSmoothing;
		[SerializeField] LegSmoothing rightLegSmoothing;
		[SerializeField, HideInInspector,] LegPoseCode leftLeg;
		[SerializeField, HideInInspector,] LegPoseCode rightLeg;
		[SerializeField] Collider leftCollider;
		[SerializeField] Collider rightCollider;
		readonly LinearSmoothing weight;
		public GroundDetect LeftGroundDetect => hand.LeftGroundDetect;
		public GroundDetect RightGroundDetect => hand.RightGroundDetect;
		public bool Jumping => hand.HandPositionUpdater.Jumping;
		public bool Crunching => hand.HandPositionUpdater.Crunching;
		public LegPoseCode LeftLeg
		{
			get => leftLeg;
			set
			{
				if (leftLeg == value) return;
				switch (value)
				{
					case LegPoseCode.Idle:
						if (leftLeg == LegPoseCode.LiftUp)
							if (hand.Left.Tip.position.GetTerrainHit().HasValue)
								leftLegSmoothing.Step(hand.Left.Tip.position.GetTerrainHit().Value, 0.05f);
						break;
					case LegPoseCode.LiftForward when hand.RaycastSource.LeftForwardHitPoint.HasValue:
						leftLegSmoothing.Step(hand.RaycastSource.LeftForwardHitPoint.Value, 0.05f);
						break;
					case LegPoseCode.LiftUp when hand.RaycastSource.LeftHitPoint.HasValue:
						leftLegSmoothing.Step(hand.RaycastSource.LeftHitPoint.Value + Vector3.up * 0.04f, 0.05f);
						break;
					case LegPoseCode.LiftBackward when hand.RaycastSource.LeftBackwardHitPoint.HasValue:
						leftLegSmoothing.Step(hand.RaycastSource.LeftBackwardHitPoint.Value, 0.05f);
						break;
				}
				leftLeg = value;
				OnLeftLegChanged?.TryInvoke();
			}
		}
		public LegPoseCode RightLeg
		{
			get => rightLeg;
			set
			{
				if (rightLeg == value) return;
				switch (value)
				{
					case LegPoseCode.Idle:
						if (rightLeg == LegPoseCode.LiftUp)
							if (hand.Right.Tip.position.GetTerrainHit().HasValue)
								rightLegSmoothing.Step(hand.Right.Tip.position.GetTerrainHit().Value, 0.05f);
						break;
					case LegPoseCode.LiftForward when hand.RaycastSource.RightForwardHitPoint.HasValue:
						rightLegSmoothing.Step(hand.RaycastSource.RightForwardHitPoint.Value, 0.05f);
						break;
					case LegPoseCode.LiftUp when hand.RaycastSource.RightHitPoint.HasValue:
						rightLegSmoothing.Step(hand.RaycastSource.RightHitPoint.Value + Vector3.up * 0.03f, 0.05f);
						break;
					case LegPoseCode.LiftBackward when hand.RaycastSource.RightBackwardHitPoint.HasValue:
						rightLegSmoothing.Step(hand.RaycastSource.RightBackwardHitPoint.Value, 0.05f);
						break;
				}
				rightLeg = value;
				OnRightLegChanged?.TryInvoke();
			}
		}
		internal float Weight { get; private set; }
		public event Action OnLeftLegChanged;
		public event Action OnRightLegChanged;
		public event Action OnJump;
		public event Action OnLanded
		{
			add => hand.HandPositionUpdater.OnLanded += value;
			remove => hand.HandPositionUpdater.OnLanded -= value;
		}
		HandIKInput() => weight = new(0.1f, v => Weight = v);
		void Awake()
		{
			leftLegSmoothing.transform.parent = transform.parent;
			rightLegSmoothing.transform.parent = transform.parent;
		}
		void OnDestroy()
		{
			leftLegSmoothing.Destroy();
			rightLegSmoothing.Destroy();
		}
		public void SetWeight(float weight, float duration) { }
		public void Crunch(bool crunch) => hand.HandPositionUpdater.Crunch(crunch);
		public void Jump(float speed, Action callback)
		{
			if (Jumping) return;
			hand.HandPositionUpdater.Jump(speed, callback);
			OnJump?.TryInvoke();
		}
	}
}
