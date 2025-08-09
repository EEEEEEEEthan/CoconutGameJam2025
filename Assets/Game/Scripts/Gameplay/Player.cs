using Game.FingerRigging;
using UnityEngine;
namespace Game.Gameplay
{
	public class Player : GameBehaviour
	{
		[SerializeField] HandIKInput handIKInput;
		[SerializeField] Animator animator;
		void Update()
		{
			if (Input.GetKey(KeyCode.E))
				handIKInput.LeftLeg = LegPoseCode.LiftForward;
			else if (Input.GetKey(KeyCode.W))
				handIKInput.LeftLeg = LegPoseCode.LiftUp;
			else if (Input.GetKey(KeyCode.Q))
				handIKInput.LeftLeg = LegPoseCode.LiftBackward;
			else
				handIKInput.LeftLeg = LegPoseCode.Idle;
			if (Input.GetKey(KeyCode.D))
				handIKInput.RightLeg = LegPoseCode.LiftForward;
			else if (Input.GetKey(KeyCode.S))
				handIKInput.RightLeg = LegPoseCode.LiftUp;
			else if (Input.GetKey(KeyCode.A))
				handIKInput.RightLeg = LegPoseCode.LiftBackward;
			else
				handIKInput.RightLeg = LegPoseCode.Idle;
			if (Input.GetKeyDown(KeyCode.Space)) handIKInput.Crunch();
			if (Input.GetKeyUp(KeyCode.Space)) handIKInput.Jump(0.1f);
		}
	}
}
