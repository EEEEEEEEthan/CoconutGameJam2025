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
		public static InputBlock operator |(InputBlock a, InputBlock b) =>
			new()
			{
				leftForward = a.leftForward || b.leftForward,
				leftUp = a.leftUp || b.leftUp,
				leftBackward = a.leftBackward || b.leftBackward,
				rightForward = a.rightForward || b.rightForward,
				rightUp = a.rightUp || b.rightUp,
				rightBackward = a.rightBackward || b.rightBackward,
				greetings = a.greetings || b.greetings,
				surprise = a.surprise || b.surprise,
				shy = a.shy || b.shy,
				angry = a.angry || b.angry,
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
		[SerializeField] LayerMask airWallLayerMask = (int)LayerMaskCode.AirWall;
		[SerializeField] float raycastDistance = 1.0f;
		[SerializeField] InputBlock inputBlock;
		[SerializeField] InputBlock locked;
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
		public InputBlock Block => inputBlock | Locked;
		public InputBlock InputBlock
		{
			get => inputBlock;
			set => inputBlock = value;
		}
		public InputBlock Locked
		{
			get => locked;
			set => locked = value;
		}
		public EmotionCode CurrentEmotion { get; private set; }
		public event Action<EmotionCode> OnEmotionTriggered;
		void Update()
		{
			if (isInSpecialAnim) return;
			handIKInput.transform.position = handIKInput.transform.position.WithZ(0);

			// 检测左右空气墙
			var hasLeftWall = Physics.Raycast(transform.position, Vector3.left, raycastDistance, airWallLayerMask);
			var hasRightWall = Physics.Raycast(transform.position, Vector3.right, raycastDistance, airWallLayerMask);
			if (Input.GetKey(KeyCode.Q) && !Block.leftForward && !hasRightWall)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftForward;
				animator.SetBool(AnimatorHashes.walkLeft, true);
				animator.SetBool(AnimatorHashes.standLeft, false);
			}
			else if (Input.GetKey(KeyCode.A) && !Block.leftUp)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftUp;
				animator.SetBool(AnimatorHashes.walkLeft, false);
				animator.SetBool(AnimatorHashes.standLeft, true);
			}
			else if (Input.GetKey(KeyCode.Z) && !Block.leftBackward && !hasLeftWall)
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
			if (Input.GetKey(KeyCode.W) && !Block.rightForward && !hasRightWall)
			{
				handIKInput.RightLeg = LegPoseCode.LiftForward;
				animator.SetBool(AnimatorHashes.walkRight, true);
				animator.SetBool(AnimatorHashes.standRight, false);
			}
			else if (Input.GetKey(KeyCode.S) && !Block.rightUp)
			{
				handIKInput.RightLeg = LegPoseCode.LiftUp;
				animator.SetBool(AnimatorHashes.walkRight, false);
				animator.SetBool(AnimatorHashes.standRight, true);
			}
			else if (Input.GetKey(KeyCode.X) && !Block.rightBackward && !hasLeftWall)
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
			if (Input.GetKeyDown(KeyCode.Alpha1) && !Block.greetings)
			{
				animator.SetTrigger(AnimatorHashes.hi);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Hi);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha2) && !Block.surprise)
			{
				animator.SetTrigger(AnimatorHashes.surprise);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Surprise);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha3) && !Block.shy)
			{
				animator.SetTrigger(AnimatorHashes.shy);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Shy);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha4) && !Block.angry)
			{
				animator.SetTrigger(AnimatorHashes.angry);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Angry);
				isInSpecialAnim = true;
			}
		}
		void OnDrawGizmos() => Gizmos.DrawRay(transform.position, Vector3.right * 100);
		public void Unlock(KeyCode key)
		{
			var locked = Locked;
			switch (key)
			{
				case KeyCode.A:
					locked.leftUp = false;
					break;
				case KeyCode.S:
					locked.rightUp = false;
					break;
				case KeyCode.Q:
					locked.leftForward = false;
					break;
				case KeyCode.W:
					locked.rightForward = false;
					break;
				case KeyCode.Z:
					locked.leftBackward = false;
					break;
				case KeyCode.X:
					locked.rightBackward = false;
					break;
				case KeyCode.Alpha1:
					locked.greetings = false;
					break;
				case KeyCode.Alpha2:
					locked.surprise = false;
					break;
				case KeyCode.Alpha3:
					locked.shy = false;
					break;
				case KeyCode.Alpha4:
					locked.angry = false;
					break;
			}
			Locked = locked;
		}
		public void SetSpecialAnimEnd() => isInSpecialAnim = false;
	}
}
