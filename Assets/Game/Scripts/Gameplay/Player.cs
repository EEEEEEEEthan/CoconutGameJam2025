using System;
using Game.FingerRigging;
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
		public enum EmotionCode
		{
			Hi,
			Surprise,
			Shy,
			Angry,
		}
		public static readonly int s_walkLeft = Animator.StringToHash("WalkLeft");
		public static readonly int s_walkRight = Animator.StringToHash("WalkRight");
		public static readonly int s_standLeft = Animator.StringToHash("StandLeft");
		public static readonly int s_standRight = Animator.StringToHash("StandRight");
		static readonly int s_hi = Animator.StringToHash(nameof(EmotionCode.Hi));
		static readonly int s_surpirse = Animator.StringToHash(nameof(EmotionCode.Surprise));
		static readonly int s_shy = Animator.StringToHash(nameof(EmotionCode.Shy));
		static readonly int s_angry = Animator.StringToHash(nameof(EmotionCode.Angry));
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
			handIKInput.transform.position = handIKInput.transform.position.WithZ(0);
			if (Input.GetKey(KeyCode.Q) && !inputBlock.leftForward)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftForward;
				animator.SetBool(s_walkLeft, true);
				animator.SetBool(s_standLeft, false);
			}
			else if (Input.GetKey(KeyCode.A) && !inputBlock.leftUp)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftUp;
				animator.SetBool(s_walkLeft, false);
				animator.SetBool(s_standLeft, true);
			}
			else if (Input.GetKey(KeyCode.Z) && !inputBlock.leftBackward)
			{
				handIKInput.LeftLeg = LegPoseCode.LiftBackward;
				animator.SetBool(s_walkLeft, false);
				animator.SetBool(s_standLeft, false);
			}
			else
			{
				handIKInput.LeftLeg = LegPoseCode.Idle;
				animator.SetBool(s_walkLeft, false);
				animator.SetBool(s_standLeft, false);
			}
			if (Input.GetKey(KeyCode.W) && !inputBlock.rightForward)
			{
				handIKInput.RightLeg = LegPoseCode.LiftForward;
				animator.SetBool(s_walkRight, true);
				animator.SetBool(s_standRight, false);
			}
			else if (Input.GetKey(KeyCode.S) && !inputBlock.rightUp)
			{
				handIKInput.RightLeg = LegPoseCode.LiftUp;
				animator.SetBool(s_walkRight, false);
				animator.SetBool(s_standRight, true);
			}
			else if (Input.GetKey(KeyCode.X) && !inputBlock.rightBackward)
			{
				handIKInput.RightLeg = LegPoseCode.LiftBackward;
				animator.SetBool(s_walkRight, false);
				animator.SetBool(s_standRight, false);
			}
			else
			{
				handIKInput.RightLeg = LegPoseCode.Idle;
				animator.SetBool(s_walkRight, false);
				animator.SetBool(s_standRight, false);
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
				animator.SetTrigger(s_hi);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Hi);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha2) && !inputBlock.surprise)
			{
				animator.SetTrigger(s_surpirse);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Surprise);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha3) && !inputBlock.shy)
			{
				animator.SetTrigger(s_shy);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Shy);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha4) && !inputBlock.angry)
			{
				animator.SetTrigger(s_angry);
				OnEmotionTriggered?.TryInvoke(CurrentEmotion = EmotionCode.Angry);
				isInSpecialAnim = true;
			}
		}
		void OnDrawGizmos() => Gizmos.DrawRay(transform.position, Vector3.right * 100);
		public void SetSpecialAnimEnd() => isInSpecialAnim = false;
	}
}
