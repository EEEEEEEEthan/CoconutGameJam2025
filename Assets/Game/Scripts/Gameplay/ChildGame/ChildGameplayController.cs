using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay.ChildGame
{
	public class ChildGameplayController : GameBehaviour
	{
		public enum EmotionCode
		{
			Idle,
			Hi,
			Please,
			Success,
			Wrong,
			A,
			AJump,
			S,
			SJump,
			Alone,
		}
		static readonly IReadOnlyList<int> emotionHashes = Enum<EmotionCode>.Values.Select(i => Animator.StringToHash(i.ToString())).ToArray();
		[SerializeField] Transform childRoot;
		[SerializeField] Animator animator;
		[SerializeField] Transform lookTarget;
		[SerializeField] Transform jump1;
		[SerializeField] Transform jump2;
		void Awake()
		{
			enabled = false;
			Emotion(EmotionCode.Alone);
		}
		void OnEnable()
		{
			StopAllCoroutines();
			StartCoroutine(Play());
			return;
			IEnumerator Play()
			{
				GameRoot.Player.InputBlock = InputBlock.all;
				Emotion(EmotionCode.Idle);
				yield return new WaitForSeconds(0.5f);
				GameRoot.CameraController.LookAt(lookTarget, 14);
				yield return new WaitForSeconds(0.5f);
				Emotion(EmotionCode.Hi);
				yield return new WaitForSeconds(1f);
				Emotion(EmotionCode.Please);
				yield return childRoot.WaitJump(jump1.position, 0.01f, 0.3f);
				yield return childRoot.WaitJump(jump2.position, 0.01f, 0.3f);
				GameRoot.CameraController.LookAtPlayer();
			}
		}
		void Emotion(EmotionCode code) => animator.SetTrigger(emotionHashes[(int)code]);
	}
}
