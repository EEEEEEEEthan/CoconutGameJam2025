using Game.Utilities;
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
		public GroundDetect LeftGroundDetect => hand.LeftGroundDetect;
		public GroundDetect RightGroundDetect => hand.RightGroundDetect;
		public LegPoseCode LeftLeg
		{
			get => leftLeg;
			set
			{
				if (leftLeg == value) return;
				switch (value)
				{
					case LegPoseCode.Idle when hand.Left.Tip.position.GetTerrainHit().HasValue:
						leftLeg = value;
						leftLegSmoothing.Step(hand.Left.Tip.position.GetTerrainHit().Value, 0.05f);
						break;
					case LegPoseCode.LiftForward when hand.RaycastSource.LeftForwardHitPoint.HasValue:
						leftLeg = value;
						leftLegSmoothing.Step(hand.RaycastSource.LeftForwardHitPoint.Value, 0.05f);
						break;
					case LegPoseCode.LiftUp when hand.RaycastSource.LeftHitPoint.HasValue:
						leftLeg = value;
						leftLegSmoothing.Step(hand.RaycastSource.LeftHitPoint.Value + Vector3.up * 0.04f, 0.05f);
						break;
					case LegPoseCode.LiftBackward when hand.RaycastSource.LeftBackwardHitPoint.HasValue:
						leftLeg = value;
						leftLegSmoothing.Step(hand.RaycastSource.LeftBackwardHitPoint.Value, 0.05f);
						break;
				}
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
					case LegPoseCode.Idle when hand.Right.Tip.position.GetTerrainHit().HasValue:
						rightLeg = value;
						rightLegSmoothing.Step(hand.Right.Tip.position.GetTerrainHit().Value, 0.05f);
						break;
					case LegPoseCode.LiftForward when hand.RaycastSource.RightForwardHitPoint.HasValue:
						rightLeg = value;
						rightLegSmoothing.Step(hand.RaycastSource.RightForwardHitPoint.Value, 0.05f);
						break;
					case LegPoseCode.LiftUp when hand.RaycastSource.RightHitPoint.HasValue:
						rightLeg = value;
						rightLegSmoothing.Step(hand.RaycastSource.RightHitPoint.Value + Vector3.up * 0.03f, 0.05f);
						break;
					case LegPoseCode.LiftBackward when hand.RaycastSource.RightBackwardHitPoint.HasValue:
						rightLeg = value;
						rightLegSmoothing.Step(hand.RaycastSource.RightBackwardHitPoint.Value, 0.05f);
						break;
				}
			}
		}
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
		public void Crunch() => hand.JumpSmoothing.Crunch();
		public void Stand() => hand.JumpSmoothing.Stand();
		public void Jump(float speed) => hand.JumpSmoothing.Jump(speed);
	}
}
