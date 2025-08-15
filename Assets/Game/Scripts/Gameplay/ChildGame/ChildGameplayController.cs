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
		[SerializeField] Animator animator;
		[SerializeField] Transform lookTarget;
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
				GameRoot.CameraController.LookAt(lookTarget, 12);
				yield return new WaitForSeconds(1.5f);
				Emotion(EmotionCode.Hi);
				yield return new WaitForSeconds(1.5f);
				GameRoot.CameraController.LookAtPlayer();
			}
		}
		void Emotion(EmotionCode code) => animator.SetTrigger(emotionHashes[(int)code]);
	}
}
