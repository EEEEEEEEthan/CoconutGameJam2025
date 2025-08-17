using System;
using Game.FingerRigging;
using Game.ResourceManagement;
using Game.Utilities;
using ReferenceHelper;
using UnityEngine;
namespace Game.Gameplay
{
	[Serializable]
	public struct InputBlock
	{
		public static readonly InputBlock all = new()
		{
			leftForward = true,
			leftUp = true,
			leftBackward = true,
			rightForward = true,
			rightUp = true,
			rightBackward = true,
			greetings = true,
			surprise = true,
			shy = true,
			angry = true,
		};
		public bool leftForward;
		public bool leftUp;
		public bool leftBackward;
		public bool rightForward;
		public bool rightUp;
		public bool rightBackward;
		public bool greetings;
		public bool surprise;
		public bool shy;
		public bool angry;
	}
	public class Player : GameBehaviour
	{
		[SerializeField] LayerMask airWallLayerMask = (int)LayerMaskCode.AirWall;
		[SerializeField] float raycastDistance = 1.0f;
		
		public static class AnimatorHashes
		{
			public static readonly int walkLeft = Animator.StringToHash("WalkLeft");
			public static readonly int walkRight = Animator.StringToHash("WalkRight");
			public static readonly int standLeft = Animator.StringToHash("StandLeft");
			public static readonly int standRight = Animator.StringToHash("StandRight");
			public static readonly int hi = Animator.StringToHash(nameof(EmotionCode.Hi));
			public static readonly int surprise = Animator.StringToHash(nameof(EmotionCode.Surprise));
			public static readonly int shy = Animator.StringToHash(nameof(EmotionCode.Shy));
			public static readonly int angry = Animator.StringToHash(nameof(EmotionCode.Angry));
		}
		public enum EmotionCode
		{
			Hi,
			Surprise,
			Shy,
			Angry,
		}
		[SerializeField] InputBlock inputBlock;
		[SerializeField, ObjectReference,] HandIKInput handIKInput;
		[SerializeField, ObjectReference("HandWithIK"),]
		Animator animator;
		[SerializeField, ObjectReference(nameof(CameraTarget)),]
		Transform cameraTarget;
		[SerializeField, ObjectReference("PlayerPosition"),]
		Collider playerPositionTrigger;
		bool isInSpecialAnim;
		public Collider PlayerPositionTrigger => playerPositionTrigger;
		public Transform CameraTarget => cameraTarget;
		public HandIKInput HandIkInput => handIKInput;
		public InputBlock InputBlock
		{
			get => inputBlock;
			set => inputBlock = value;
		}
		public EmotionCode CurrentEmotion { get; private set; }
		public event Action<EmotionCode> OnEmotionTriggered;
		void Update()
		{
			if (isInSpecialAnim) return;
			handIKInput.transform.position = handIKInput.transform.position.WithZ(0);
			
			// 检测左右空气墙
			bool hasLeftWall = Physics.Raycast(transform.position, Vector3.left, raycastDistance, airWallLayerMask);
			bool hasRightWall = Physics.Raycast(transform.position, Vector3.right, raycastDistance, airWallLayerMask);
			
			if (Input.GetKey(KeyCode.Q) && !inputBlock.leftForward && !hasRightWall)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftForward;
				animator.SetBool(AnimatorHashes.walkLeft, true);
				animator.SetBool(AnimatorHashes.standLeft, false);
			}
			else if (Input.GetKey(KeyCode.A) && !inputBlock.leftUp)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftUp;
				animator.SetBool(AnimatorHashes.walkLeft, false);
				animator.SetBool(AnimatorHashes.standLeft, true);
			}
			else if (Input.GetKey(KeyCode.Z) && !inputBlock.leftBackward && !hasLeftWall)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftBackward;
				animator.SetBool(AnimatorHashes.walkLeft, false);
				animator.SetBool(AnimatorHashes.standLeft, false);
			}
			else
			{
				handIKInput.LeftLeg = LegPoseCode.Idle;
				animator.SetBool(AnimatorHashes.walkLeft, false);
				animator.SetBool(AnimatorHashes.standLeft, false);
			}
			if (Input.GetKey(KeyCode.W) && !inputBlock.rightForward && !hasRightWall)
			{
				handIKInput.RightLeg = LegPoseCode.LiftForward;
				animator.SetBool(AnimatorHashes.walkRight, true);
				animator.SetBool(AnimatorHashes.standRight, false);
			}
			else if (Input.GetKey(KeyCode.S) && !inputBlock.rightUp)
			{
				handIKInput.RightLeg = LegPoseCode.LiftUp;
				animator.SetBool(AnimatorHashes.walkRight, false);
				animator.SetBool(AnimatorHashes.standRight, true);
			}
			else if (Input.GetKey(KeyCode.X) && !inputBlock.rightBackward && !hasLeftWall)
			{
				handIKInput.RightLeg = LegPoseCode.LiftBackward;
				animator.SetBool(AnimatorHashes.walkRight, false);
				animator.SetBool(AnimatorHashes.standRight, false);
			}
			else
			{
				handIKInput.RightLeg = LegPoseCode.Idle;
				animator.SetBool(AnimatorHashes.walkRight, false);
				animator.SetBool(AnimatorHashes.standRight, false);
			}
			/*
			if (Input.GetKeyDown(KeyCode.Space)) handIKInput.Crunch(true);
			if (Input.GetKeyUp(KeyCode.Space))
			{
				handIKInput.Crunch(false);
				handIKInput.Jump(1, () => Debug.Log("Landed!"));
			}
			*/
			if (isInSpecialAnim) return;
			if (Input.GetKeyDown(KeyCode.Alpha1) && !inputBlock.greetings)
			{
				animator.SetTrigger(AnimatorHashes.hi);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Hi);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha2) && !inputBlock.surprise)
			{
				animator.SetTrigger(AnimatorHashes.surprise);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Surprise);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha3) && !inputBlock.shy)
			{
				animator.SetTrigger(AnimatorHashes.shy);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Shy);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha4) && !inputBlock.angry)
			{
				animator.SetTrigger(AnimatorHashes.angry);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Angry);
				isInSpecialAnim = true;
			}
		}
		void OnDrawGizmos() => Gizmos.DrawRay(transform.position, Vector3.right * 100);
		public void SetSpecialAnimEnd() => isInSpecialAnim = false;
	}
}
