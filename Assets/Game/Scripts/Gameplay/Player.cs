using Game.FingerRigging;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay
{
	public class Player : GameBehaviour
	{
		static readonly int s_walkLeft = Animator.StringToHash("WalkLeft");
		static readonly int s_walkRight = Animator.StringToHash("WalkRight");
		static readonly int s_standLeft = Animator.StringToHash("StandLeft");
		static readonly int s_standRight = Animator.StringToHash("StandRight");
		static readonly int s_hi = Animator.StringToHash("Hi");
		static readonly int s_surpirse = Animator.StringToHash("Surprise");
		static readonly int s_shy = Animator.StringToHash("Shy");
		static readonly int s_angry = Animator.StringToHash("Angry");
		bool isInSpecialAnim = false;
		[SerializeField] HandIKInput handIKInput;
		[SerializeField] Animator animator;
		public HandIKInput HandIkInput => handIKInput;
		void Awake()
		{
			handIKInput.LeftGroundDetect.OnTriggerEntered += collider => Debug.Log($"left trigger entered: {collider.name}");
			handIKInput.LeftGroundDetect.OnTriggerExited += collider => Debug.Log($"left trigger exited: {collider.name}");
			handIKInput.RightGroundDetect.OnTriggerEntered += collider => Debug.Log($"right trigger entered: {collider.name}");
			handIKInput.RightGroundDetect.OnTriggerExited += collider => Debug.Log($"right trigger exited: {collider.name}");
			GameRoot.WaterGame.OnLevelCompleted += level =>
			{
				GUIDebug.CreateWindow($"You completed level {level}",
					close =>
					{
						GUILayout.Label("You can listen to GameRoot.WaterGame.OnLevelCompleted event to handle level completion logic.", GUILayout.Width(400));
						GUILayout.Label("Next level automatically opened. See WaterGame/WaterGameJudge");
						if (GUILayout.Button("Got it")) close();
					});
			};
		}
		void Update()
		{
			handIKInput.transform.position = handIKInput.transform.position.WithZ(0);

			if (Input.GetKey(KeyCode.Q))
			{
				handIKInput.LeftLeg = LegPoseCode.LiftForward;
				animator.SetBool(s_walkLeft, true);
				animator.SetBool(s_standLeft, false);
			}
			else if (Input.GetKey(KeyCode.A))
			{
				handIKInput.LeftLeg = LegPoseCode.LiftUp;
				animator.SetBool(s_walkLeft, false);
				animator.SetBool(s_standLeft, true);
			}
			else if (Input.GetKey(KeyCode.Z))
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
			if (Input.GetKey(KeyCode.W))
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
			else if (Input.GetKey(KeyCode.X))
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
			if (Input.GetKeyDown(KeyCode.Space)) handIKInput.Crunch(true);
			if (Input.GetKeyUp(KeyCode.Space))
			{
				handIKInput.Crunch(false);
				handIKInput.Jump(1, () => Debug.Log("Landed!"));
			}
			if (isInSpecialAnim) return;
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				animator.SetTrigger(s_hi);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				animator.SetTrigger(s_surpirse);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				animator.SetTrigger(s_shy);
				isInSpecialAnim = true;
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				animator.SetTrigger(s_angry);
				isInSpecialAnim = true;
			}
		}
		public void SetSpecialAnimEnd()
		{
			isInSpecialAnim = false;
		}
	}
}
