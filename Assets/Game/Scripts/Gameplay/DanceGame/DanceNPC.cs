using System.Collections;
using System.Collections.Generic;
using Game.FingerRigging;
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
		IEnumerator DanceCoroutine(List<NoteData> noteDataList)
		{
			IsDancing = true;
			var startTime = Time.time;
			var noteIndex = 0;
			while (noteIndex < noteDataList.Count && IsDancing)
			{
				var currentTime = Time.time - startTime;
				var noteData = noteDataList[noteIndex];
				if (currentTime >= noteData.time)
				{
					ExecuteAction(noteData.key);
					noteIndex++;
					yield return new WaitForSeconds(actionDuration);
					animator.SetBool(Player.s_walkLeft, false);
					animator.SetBool(Player.s_walkRight, false);
					animator.SetBool(Player.s_standLeft, false);
					animator.SetBool(Player.s_standRight, false);
				}
				else
				{
					yield return null;
				}
			}
			IsDancing = false;
			Debug.Log("DanceNPC: 舞蹈完成");
		}
		void ExecuteAction(KeyCode keyCode)
		{
			switch (keyCode)
			{
				case KeyCode.Q:
					handIkInput.LeftLeg = LegPoseCode.LiftForward;
					break;
				case KeyCode.A:
					handIkInput.LeftLeg = LegPoseCode.LiftUp;
					break;
				case KeyCode.Z:
					handIkInput.LeftLeg = LegPoseCode.LiftBackward;
					break;
				case KeyCode.W:
					handIkInput.RightLeg = LegPoseCode.LiftForward;
					break;
				case KeyCode.S:
					handIkInput.RightLeg = LegPoseCode.LiftUp;
					break;
				case KeyCode.X:
					handIkInput.RightLeg = LegPoseCode.LiftBackward;
					break;
				case KeyCode.Alpha1:
					animator.SetBool(Player.s_walkLeft, true);
					animator.SetBool(Player.s_standLeft, false);
					break;
				case KeyCode.Alpha2:
					animator.SetBool(Player.s_walkRight, true);
					animator.SetBool(Player.s_standRight, false);
					break;
				case KeyCode.Alpha3:
					animator.SetBool(Player.s_standLeft, true);
					animator.SetBool(Player.s_walkLeft, false);
					break;
				case KeyCode.Alpha4:
					animator.SetBool(Player.s_standRight, true);
					animator.SetBool(Player.s_walkRight, false);
					break;
				default:
					Debug.LogWarning($"DanceNPC: 未识别的按键 {keyCode}");
					break;
			}
			Debug.Log($"DanceNPC: 执行动作 {keyCode}");
		}
	}
}
