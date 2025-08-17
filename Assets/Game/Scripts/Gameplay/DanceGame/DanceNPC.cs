using System;
using System.Collections;
using System.Collections.Generic;
using Game.FingerRigging;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay.DanceGame
{
	public class DanceNPC : GameBehaviour
	{
		[SerializeField] HandIKInput handIkInput;
		[SerializeField] Animator animator;
		[SerializeField] float actionDuration = 0.5f;
		Coroutine danceCoroutine;
		public bool IsDancing { get; private set; }
		void Update()
		{
			transform.position = transform.position.WithZ(0);
		}
		void Awake() => transform.parent = GameRoot.transform;
		void OnDisable() => StopDance();
		public void Dance(List<NoteData> noteDataList)
		{
			if (IsDancing) StopCoroutine(danceCoroutine);
			if (noteDataList == null || noteDataList.Count == 0)
			{
				Debug.LogWarning("DanceNPC: 舞谱数据为空");
				return;
			}
			if (handIkInput == null)
			{
				Debug.LogError("DanceNPC: HandIkInput未分配");
				return;
			}
			danceCoroutine = StartCoroutine(DanceCoroutine(noteDataList));
		}
		public void StopDance()
		{
			if (IsDancing && danceCoroutine != null)
			{
				StopCoroutine(danceCoroutine);
				IsDancing = false;
			}
		}
		IEnumerator Start()
		{
			handIkInput.LeftLeg = LegPoseCode.LiftUp;
			yield return new WaitForSeconds(0.5f);
			handIkInput.LeftLeg = LegPoseCode.Idle;
			yield return new WaitForSeconds(0.5f);
			handIkInput.RightLeg = LegPoseCode.LiftUp;
			yield return new WaitForSeconds(0.5f);
			handIkInput.RightLeg = LegPoseCode.Idle;
		}
		IEnumerator DanceCoroutine(List<NoteData> noteDataList)
		{
			IsDancing = true;
			var startTime = Time.time;
			var noteIndex = 0;
			while (noteIndex < noteDataList.Count && IsDancing)
			{
				while (true)
				{
					var currentTime = Time.time - startTime;
					var noteData = noteDataList[noteIndex];
					if (currentTime >= noteData.time)
					{
						ExecuteAction(noteData.key);
						noteIndex++;
						break;
					}
					yield return null;
				}
				yield return null;
			}
			IsDancing = false;
			Debug.Log("DanceNPC: 舞蹈完成");
		}
		void ExecuteAction(KeyCode keyCode)
		{
			switch (keyCode)
			{
				case KeyCode.Q:
					animator.SetBool(Player.s_walkLeft, true);
					animator.SetBool(Player.s_standLeft, false);
					handIkInput.LeftLeg = LegPoseCode.LiftForward;
					handIkInput.RightLeg = LegPoseCode.Idle;
					break;
				case KeyCode.A:
					animator.SetBool(Player.s_walkLeft, false);
					animator.SetBool(Player.s_standLeft, true);
					handIkInput.LeftLeg = LegPoseCode.LiftUp;
					handIkInput.RightLeg = LegPoseCode.Idle;
					break;
				case KeyCode.Z:
					animator.SetBool(Player.s_walkLeft, false);
					animator.SetBool(Player.s_standLeft, false);
					handIkInput.LeftLeg = LegPoseCode.LiftBackward;
					handIkInput.RightLeg = LegPoseCode.Idle;
					break;
				case KeyCode.W:
					animator.SetBool(Player.s_walkRight, true);
					animator.SetBool(Player.s_standRight, false);
					handIkInput.LeftLeg = LegPoseCode.Idle;
					handIkInput.RightLeg = LegPoseCode.LiftForward;
					break;
				case KeyCode.S:
					animator.SetBool(Player.s_walkRight, false);
					animator.SetBool(Player.s_standRight, true);
					handIkInput.LeftLeg = LegPoseCode.Idle;
					handIkInput.RightLeg = LegPoseCode.LiftUp;
					break;
				case KeyCode.X:
					animator.SetBool(Player.s_walkRight, false);
					animator.SetBool(Player.s_standRight, false);
					handIkInput.LeftLeg = LegPoseCode.Idle;
					handIkInput.RightLeg = LegPoseCode.LiftBackward;
					break;
				case KeyCode.Alpha1:
					animator.SetBool(Player.s_walkLeft, false);
					animator.SetBool(Player.s_standLeft, false);
					animator.SetBool(Player.s_walkRight, false);
					animator.SetBool(Player.s_standRight, false);
					animator.SetTrigger(Player.s_hi);
					handIkInput.LeftLeg = LegPoseCode.Idle;
					handIkInput.RightLeg = LegPoseCode.Idle;
					break;
				case KeyCode.Alpha2:
					animator.SetBool(Player.s_walkLeft, false);
					animator.SetBool(Player.s_standLeft, false);
					animator.SetBool(Player.s_walkRight, false);
					animator.SetBool(Player.s_standRight, false);
					animator.SetTrigger(Player.s_surpirse);
					handIkInput.LeftLeg = LegPoseCode.Idle;
					handIkInput.RightLeg = LegPoseCode.Idle;
					break;
				case KeyCode.Alpha3:
					animator.SetBool(Player.s_walkLeft, false);
					animator.SetBool(Player.s_standLeft, false);
					animator.SetBool(Player.s_walkRight, false);
					animator.SetBool(Player.s_standRight, false);
					animator.SetTrigger(Player.s_shy);
					handIkInput.LeftLeg = LegPoseCode.Idle;
					handIkInput.RightLeg = LegPoseCode.Idle;
					break;
				case KeyCode.Alpha4:
					animator.SetBool(Player.s_walkLeft, false);
					animator.SetBool(Player.s_standLeft, false);
					animator.SetBool(Player.s_walkRight, false);
					animator.SetBool(Player.s_standRight, false);
					animator.SetTrigger(Player.s_angry);
					handIkInput.LeftLeg = LegPoseCode.Idle;
					handIkInput.RightLeg = LegPoseCode.Idle;
					break;
				default:
					animator.SetBool(Player.s_walkLeft, false);
					animator.SetBool(Player.s_standLeft, false);
					animator.SetBool(Player.s_walkRight, false);
					animator.SetBool(Player.s_standRight, false);
					handIkInput.LeftLeg = LegPoseCode.Idle;
					handIkInput.RightLeg = LegPoseCode.Idle;
					break;
			}
			Debug.Log($"DanceNPC: 执行动作 {keyCode}");
		}
	}
}
