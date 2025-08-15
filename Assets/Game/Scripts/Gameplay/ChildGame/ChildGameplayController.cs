using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.WaterGame;
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
		[SerializeField] MeshCollider groundMesh;
		public bool PlayerInWaterArea { get; set; }
		void Awake()
		{
			enabled = false;
			Emotion(EmotionCode.Alone);
		}
		void OnDisable()
		{
			PlayerInWaterArea = false;
		}
		void OnEnable()
		{
			StopAllCoroutines();
			StartCoroutine(Play());
			return;
			IEnumerator Play()
			{
				GameRoot.Player.InputBlock = InputBlock.all;
				groundMesh.enabled = false;
				Emotion(EmotionCode.Idle);
				yield return new WaitForSeconds(0.5f);
				GameRoot.CameraController.LookAt(lookTarget, 14);
				yield return new WaitForSeconds(0.5f);
				Emotion(EmotionCode.Hi);
				yield return new WaitForSeconds(1f);
				Emotion(EmotionCode.Please);
				yield return childRoot.WaitJump(jump1.position, 0.01f, 0.3f);
				yield return childRoot.WaitJump(jump2.position, 0.01f, 0.3f);
				GameRoot.Player.InputBlock = default;
				yield return new WaitUntil(() => PlayerInWaterArea);
				GameRoot.GameCanvas.Filmic(true);
				
				
				
				
				groundMesh.enabled = true;
				GameRoot.GameCanvas.Filmic(false);
				GameRoot.CameraController.LookAtPlayer();
			}
		}
		void Emotion(EmotionCode code) => animator.SetTrigger(emotionHashes[(int)code]);
		IEnumerator<bool?> WaitForInputSequence(ActionData[] sequence)
		{
			// 检测玩家是否以指定序列输入.
			// 1.输入少于0.1秒,且这一个输入不匹配,需要忽略这一个输入
			// 2.输入之后的1秒内没有其他输入,则认为输入结束
			// 3.无任何输入,3秒后得到null
			var startTime = Time.time;
			var deadline = startTime + 3f;
			while (true)
			{
				if (Time.time > deadline)
				{
					yield return null;
					yield break;
				}
				/* if matched
				 {
				   yield return true;
				   yield break;
				 }
				*/
				yield return null;
			}
			yield break;
		}
	}
}
