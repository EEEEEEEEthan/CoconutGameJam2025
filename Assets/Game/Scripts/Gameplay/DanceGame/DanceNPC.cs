using System;
using System.Collections;
using System.Collections.Generic;
using Game.FingerRigging;
using UnityEngine;
namespace Game.Gameplay.DanceGame
{
	public class DanceNPC : MonoBehaviour
	{
		[SerializeField] HandIKInput handIkInput;
		[SerializeField] float actionDuration = 0.5f;
		Coroutine danceCoroutine;
		bool isDancing;
		public bool IsDancing => isDancing;
		public void Dance(List<NoteData> noteDataList)
		{
			if (isDancing)
			{
				StopCoroutine(danceCoroutine);
				ResetPose();
			}
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
			if (isDancing && danceCoroutine != null)
			{
				StopCoroutine(danceCoroutine);
				ResetPose();
				isDancing = false;
			}
		}
		IEnumerator DanceCoroutine(List<NoteData> noteDataList)
		{
			isDancing = true;
			var startTime = Time.time;
			var noteIndex = 0;
			while (noteIndex < noteDataList.Count && isDancing)
			{
				var currentTime = Time.time - startTime;
				var noteData = noteDataList[noteIndex];
				if (currentTime >= noteData.time)
				{
					ExecuteAction(noteData.key);
					noteIndex++;
					yield return new WaitForSeconds(actionDuration);
					ResetPose();
				}
				else
				{
					yield return null;
				}
			}
			isDancing = false;
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
				case KeyCode.D:
					//handIkInput.Jump(1.0f, null);
					break;
				default:
					Debug.LogWarning($"DanceNPC: 未识别的按键 {keyCode}");
					break;
			}
			Debug.Log($"DanceNPC: 执行动作 {keyCode}");
		}
		void ResetPose()
		{
			handIkInput.LeftLeg = LegPoseCode.Idle;
			handIkInput.RightLeg = LegPoseCode.Idle;
		}
		void OnDisable()
		{
			StopDance();
		}
	}
}
