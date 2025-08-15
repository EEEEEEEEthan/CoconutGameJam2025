using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.FingerRigging;
using Game.Gameplay.Triggers;
using Game.Gameplay.WaterGame;
using Game.Utilities;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Gameplay.ChildGame
{
	public class ChildGameplayController : GameBehaviour
	{
		enum EmotionCode
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
		[SerializeField] PlayerDetector danceArea;
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
				yield return new WaitForSeconds(0.5f);
				GameRoot.Player.InputBlock = default;
				GameRoot.CameraController.LookAtPlayer();
				yield return new WaitUntil(() => danceArea.PlayerInside);
				foreach (var collider in GameRoot.GroundColliders) collider.enabled = false;
				GameRoot.GameCanvas.Filmic(true);
				while (true)
				{
					Emotion(EmotionCode.S);
					yield return new WaitForSeconds(0.5f);
					Emotion(EmotionCode.Idle);
					yield return new WaitForSeconds(0.5f);
					Emotion(EmotionCode.A);
					yield return new WaitForSeconds(0.5f);
					Emotion(EmotionCode.Idle);
					yield return new WaitForSeconds(1);
					var result = false;
					yield return WaitForInputSequence(new[]
						{
							new ActionData
							{
								left = LegPoseCode.LiftUp,
								right = LegPoseCode.Idle,
							},
							new ActionData
							{
								left = LegPoseCode.Idle,
								right = LegPoseCode.Idle,
							},
							new ActionData
							{
								left = LegPoseCode.Idle,
								right = LegPoseCode.LiftUp,
							},
							new ActionData
							{
								left = LegPoseCode.Idle,
								right = LegPoseCode.Idle,
							},
						},
						r => result = r);
					Debug.Log($"Input sequence result: {result}");
					if (result)
					{
						yield return new WaitForSeconds(0.1f);
						Emotion(EmotionCode.Success);
						yield return new WaitForSeconds(0.8f);
						break;
					}
					yield return new WaitForSeconds(0.5f);
					Emotion(EmotionCode.Wrong);
					yield return new WaitForSeconds(1);
					yield return null;
				}
				foreach (var collider in GameRoot.GroundColliders) collider.enabled = true;
				GameRoot.GameCanvas.Filmic(false);
				GameRoot.CameraController.LookAtPlayer();
			}
		}
		void Emotion(EmotionCode code) => animator.SetTrigger(emotionHashes[(int)code]);
		IEnumerator WaitForInputSequence(ActionData[] sequence, Action<bool> callback)
		{
			const float threshold = 0.1f;
			var startTime = Time.time;
			var deadline = startTime + 3f;
			var player = GameRoot.Player;
			List<ActionData> input = new();
			player.OnEmotionTriggered += onEmotionTriggered;
			player.HandIkInput.OnLeftLegChanged += onAnyInput;
			player.HandIkInput.OnRightLegChanged += onAnyInput;
			using var disposable = new Disposable(() =>
				{
					player.OnEmotionTriggered -= onEmotionTriggered;
					player.HandIkInput.OnLeftLegChanged -= onAnyInput;
					player.HandIkInput.OnRightLegChanged -= onAnyInput;
				}
			);
			while (Time.time < deadline)
			{
				if (check() == false) break;
				yield return null;
			}
			if(input.Count > 0)
			{
				var last = input[^1];
				last.endTime = Time.time;
				input[^1] = last;
			}
			using (StringBuilderPoolThreaded.Rent(out var builder))
			{
				builder.Append("Input sequence: ");
				foreach (var i in input) builder.Append(i.ToString());
				builder.Append(" | Required: ");
				foreach (var i in sequence) builder.Append(i.ToString());
				Debug.Log(builder, this);
			}
			callback?.TryInvoke(check() == true);
			yield break;
			bool? check()
			{
				for (var i = 0; i < sequence.Length; ++i)
				{
					if (input.Count <= i) return null;
					var required = sequence[i];
					var inputAction = input[i];
					if (inputAction.endTime == 0) return null;
					if (!ActionData.Match(required, inputAction)) return false;
				}
				return true;
			}
			void onEmotionTriggered(Player.EmotionCode emotion) => onAnyInput();
			void onAnyInput()
			{
				if (input.Count > 0)
				{
					var last = input[^1];
					last.endTime = Time.time;
					if (last.endTime - last.startTime > threshold) input[^1] = last;
				}
				input.Add(new()
				{
					startTime = Time.time,
					left = player.HandIkInput.LeftLeg,
					right = player.HandIkInput.RightLeg,
				});
				deadline = Time.time + 1;
			}
		}
	}
}
