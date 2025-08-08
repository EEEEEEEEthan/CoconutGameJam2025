using System;
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
						leftLegSmoothing.SetStep(hand.Left.Tip.position.GetTerrainHit().Value, default);
						break;
					case LegPoseCode.LiftForward when hand.RaycastSource.LeftForwardHitPoint.HasValue:
						leftLeg = value;
						leftLegSmoothing.SetStep(hand.RaycastSource.LeftForwardHitPoint.Value, Vector3.down * 0.01f);
						break;
					case LegPoseCode.LiftUp when hand.RaycastSource.LeftHitPoint.HasValue:
						leftLeg = value;
						leftLegSmoothing.SetStep(hand.RaycastSource.LeftHitPoint.Value + Vector3.up * 0.5f, Vector3.down * 0.01f);
						break;
					case LegPoseCode.LiftBackward when hand.RaycastSource.LeftBackwardHitPoint.HasValue:
						leftLeg = value;
						leftLegSmoothing.SetStep(hand.RaycastSource.LeftBackwardHitPoint.Value, Vector3.down * 0.01f);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(value), value, null);
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
		}
	}
}
