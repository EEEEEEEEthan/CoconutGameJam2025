using System;
using Game.Utilities;
using Game.Utilities.Smoothing;
using ReferenceHelper;
using UnityEngine;
namespace Game.FingerRigging
{
	public class HandIKInput : MonoBehaviour
	{
		[SerializeField, Range(0, 1),] float weight = 1;
		[SerializeField, ObjectReference,] Hand hand;
		[SerializeField, ObjectReference,] HandPositionUpdater handPositionUpdater;
		[SerializeField] LegSmoothing leftLegSmoothing;
		[SerializeField] LegSmoothing rightLegSmoothing;
		[SerializeField, HideInInspector,] LegPoseCode leftLeg;
		[SerializeField, HideInInspector,] LegPoseCode rightLeg;
		[SerializeField] Collider leftCollider;
		[SerializeField] Collider rightCollider;
		readonly LinearSmoothing weightSmoothing;
		public Collider LeftCollider => leftCollider;
		public Collider RightCollider => rightCollider;
		/// <summary>获取当前IK权重值</summary>
		/// <value>IK权重，范围0-1，0表示完全不使用IK，1表示完全由IK控制</value>
		public float Weight => weight;
		/// <summary>获取当前是否处于跳跃状态</summary>
		/// <value>true表示正在跳跃，false表示未跳跃</value>
		public bool Jumping => hand.HandPositionUpdater.Jumping;
		/// <summary>获取当前是否处于蹲伏状态</summary>
		/// <value>true表示正在蹲伏，false表示未蹲伏</value>
		public bool Crunching => hand.HandPositionUpdater.Crunching;
		/// <summary>获取或设置左腿的姿态代码</summary>
		/// <value>左腿当前的姿态状态</value>
		public LegPoseCode LeftLeg
		{
			get => leftLeg;
			set => SetLegPose(true, value);
		}
		/// <summary>获取或设置右腿的姿态代码</summary>
		/// <value>右腿当前的姿态状态</value>
		public LegPoseCode RightLeg
		{
			get => rightLeg;
			set => SetLegPose(false, value);
		}
		/// <summary>左腿姿态改变时触发的事件</summary>
		public event Action OnLeftLegChanged;
		/// <summary>右腿姿态改变时触发的事件</summary>
		public event Action OnRightLegChanged;
		/// <summary>跳跃开始时触发的事件</summary>
		public event Action OnJump;
		/// <summary>着陆时触发的事件</summary>
		public event Action OnLanded
		{
			add => hand.HandPositionUpdater.OnLanded += value;
			remove => hand.HandPositionUpdater.OnLanded -= value;
		}
		HandIKInput() => weightSmoothing = new(1, v => weight = v);
		void Awake()
		{
			leftLegSmoothing.transform.parent = transform.parent;
			rightLegSmoothing.transform.parent = transform.parent;
			weight = weightSmoothing.Value;
		}
		void OnDestroy()
		{
			leftLegSmoothing.Destroy();
			rightLegSmoothing.Destroy();
		}
		/// <summary>
		///     设置ik权重
		/// </summary>
		/// <param name="weight">0-完全不用ik 1-ik完全控制</param>
		/// <param name="duration">切换ik权重的过渡时间(秒)</param>
		public void SetWeight(float weight, float duration) => weightSmoothing.Set(weight, duration);
		/// <summary>设置蹲伏状态</summary>
		/// <param name="crunch">true表示开始蹲伏，false表示结束蹲伏</param>
		public void Crunch(bool crunch) => hand.HandPositionUpdater.Crunch(crunch);
		/// <summary>执行跳跃动作</summary>
		/// <param name="speed">跳跃速度</param>
		/// <param name="callback">跳跃完成后的回调函数</param>
		public void Jump(float speed, Action callback)
		{
			if (Jumping) return;
			hand.HandPositionUpdater.Jump(speed, callback);
			OnJump?.TryInvoke();
		}
		public void SyncAnimationToIK()
		{
			SetIdlePosition(hand.Left, true);
			SetIdlePosition(hand.Right, true);
			leftLeg = LegPoseCode.Idle;
			rightLeg = LegPoseCode.Idle;
		}
		/// <summary>设置腿部姿态的通用方法</summary>
		/// <param name="isLeft">true表示左腿，false表示右腿</param>
		/// <param name="newPose">新的姿态代码</param>
		void SetLegPose(bool isLeft, LegPoseCode newPose)
		{
			var currentPose = isLeft ? leftLeg : rightLeg;
			if (currentPose == newPose) return;
			var legSmoothing = isLeft ? leftLegSmoothing : rightLegSmoothing;
			var handSide = isLeft ? hand.Left : hand.Right;
			var raycastSource = hand.RaycastSource;
			var liftUpOffset = isLeft ? 0.04f : 0.03f;
			switch (newPose)
			{
				case LegPoseCode.Idle:
					if (currentPose == LegPoseCode.LiftUp) SetIdlePosition(handSide, false);
					break;
				case LegPoseCode.LiftForward:
					var forwardHitPoint = isLeft ? raycastSource.LeftForwardHitPoint : raycastSource.RightForwardHitPoint;
					if (forwardHitPoint.HasValue) legSmoothing.Step(forwardHitPoint.Value, 0.05f);
					break;
				case LegPoseCode.LiftUp:
					var hitPoint = isLeft ? raycastSource.LeftHitPoint : raycastSource.RightHitPoint;
					if (hitPoint.HasValue) legSmoothing.Step(hitPoint.Value + Vector3.up * liftUpOffset, 0.05f);
					break;
				case LegPoseCode.LiftBackward:
					var backwardHitPoint = isLeft ? raycastSource.LeftBackwardHitPoint : raycastSource.RightBackwardHitPoint;
					if (backwardHitPoint.HasValue) legSmoothing.Step(backwardHitPoint.Value, 0.05f);
					break;
			}
			if (isLeft)
			{
				leftLeg = newPose;
				OnLeftLegChanged?.TryInvoke();
			}
			else
			{
				rightLeg = newPose;
				OnRightLegChanged?.TryInvoke();
			}
		}
		void SetIdlePosition(Finger finger, bool immediate)
		{
			var hit = finger.Tip.position.GetTerrainHit();
			if (hit.HasValue)
			{
				var legSmoothing = finger == hand.Left ? leftLegSmoothing : rightLegSmoothing;
				if (immediate)
					legSmoothing.SetPositionImmediate(hit.Value);
				else
					legSmoothing.Step(hit.Value, 0.05f);
			}
		}
	}
}
