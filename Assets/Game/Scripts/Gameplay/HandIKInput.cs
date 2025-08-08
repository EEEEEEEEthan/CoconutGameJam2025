using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay
{
	public class HandIKInput : MonoBehaviour
	{
		public enum LegPoseCode
		{
			Idle,
			LiftForward,
			LiftUp,
			LiftBackward,
		}
		[SerializeField, ObjectReference,] Hand hand;
		[SerializeField, ObjectReference,] HandPositionUpdater handPositionUpdater;
		[SerializeField] LegSmoothing leftLegSmoothing;
		[SerializeField] LegSmoothing rightLegSmoothing;
		[SerializeField, HideInInspector,] LegPoseCode leftLeg;
		[SerializeField, HideInInspector,] LegPoseCode rightLeg;
		LegPoseCode LeftLeg
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
						leftLegSmoothing.Step(hand.RaycastSource.LeftHitPoint.Value + Vector3.up * 0.03f, 0.05f);
						break;
					case LegPoseCode.LiftBackward when hand.RaycastSource.LeftBackwardHitPoint.HasValue:
						leftLeg = value;
						leftLegSmoothing.Step(hand.RaycastSource.LeftBackwardHitPoint.Value, 0.05f);
						break;
				}
			}
		}
		LegPoseCode RightLeg
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
		void Update()
		{
			if (Input.GetKey(KeyCode.E))
				LeftLeg = LegPoseCode.LiftForward;
			else if (Input.GetKey(KeyCode.W))
				LeftLeg = LegPoseCode.LiftUp;
			else if (Input.GetKey(KeyCode.Q))
				LeftLeg = LegPoseCode.LiftBackward;
			else
				LeftLeg = LegPoseCode.Idle;
			if (Input.GetKey(KeyCode.D))
				RightLeg = LegPoseCode.LiftForward;
			else if (Input.GetKey(KeyCode.S))
				RightLeg = LegPoseCode.LiftUp;
			else if (Input.GetKey(KeyCode.A))
				RightLeg = LegPoseCode.LiftBackward;
			else
				RightLeg = LegPoseCode.Idle;
		}
	}
}
