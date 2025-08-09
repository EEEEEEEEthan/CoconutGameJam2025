using Game.FingerRigging;
using UnityEngine;
namespace Game.Gameplay
{
	public class Player : GameBehaviour
	{
		[SerializeField] HandIKInput handIKInput;
		[SerializeField] Animator animator;
		static readonly int s_walkLeft = Animator.StringToHash("WalkLeft");
		static readonly int s_walkRight = Animator.StringToHash("WalkRight");
		static readonly int s_standLeft = Animator.StringToHash("StandLeft");
		static readonly int s_standRight = Animator.StringToHash("StandRight");
		void Awake()
		{
			handIKInput.LeftGroundDetect.OnTriggerEntered += collider => Debug.Log($"left trigger entered: {collider.name}");
			handIKInput.LeftGroundDetect.OnTriggerExited += collider => Debug.Log($"left trigger exited: {collider.name}");
			handIKInput.RightGroundDetect.OnTriggerEntered += collider => Debug.Log($"right trigger entered: {collider.name}");
			handIKInput.RightGroundDetect.OnTriggerExited += collider => Debug.Log($"right trigger exited: {collider.name}");
		}
		void Update()
		{
			if (Input.GetKey(KeyCode.E))
			{
				handIKInput.LeftLeg = LegPoseCode.LiftForward;
				animator.SetBool(s_walkLeft, true);
				animator.SetBool(s_standLeft, false);
			}
			else if (Input.GetKey(KeyCode.W))
			{
				handIKInput.LeftLeg = LegPoseCode.LiftUp;
				animator.SetBool(s_walkLeft, false);
				animator.SetBool(s_standLeft, true);
			}
			else if (Input.GetKey(KeyCode.Q))
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
			if (Input.GetKey(KeyCode.D))
			{
				handIKInput.RightLeg = LegPoseCode.LiftForward;
				animator.SetBool(s_walkRight, true);
				animator.SetBool(s_standRight, false);
			}
			else if (Input.GetKey(KeyCode.S))
			{
				handIKInput.RightLeg = LegPoseCode.LiftUp;
				animator.SetBool(s_walkRight, false);
				animator.SetBool(s_standRight, true);
			}
			else if (Input.GetKey(KeyCode.A))
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
			if (Input.GetKeyDown(KeyCode.Space)) handIKInput.Crunch();
			if (Input.GetKeyUp(KeyCode.Space)) handIKInput.Jump(0.1f);
		}
	}
}
