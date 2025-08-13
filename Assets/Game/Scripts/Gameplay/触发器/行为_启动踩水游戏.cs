using System;
using System.Collections.Generic;
using Game.FingerRigging;
using Game.Gameplay.WaterGame;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Gameplay
{
	public class 行为_启动踩水游戏1 : GameBehaviour
	{
		[SerializeField] Light lightBoy;
		[SerializeField] Light lightPlayer;
		[SerializeField] Animator boy;
		async void OnEnable()
		{
			while (enabled)
			{
				// 男孩表演阶段
				lightBoy.enabled = true;
				await MainThreadTimerManager.Await(2);
				boy.SetTrigger("S");
				await MainThreadTimerManager.Await(1);
				boy.SetTrigger("A");
				await MainThreadTimerManager.Await(3);
				lightBoy.enabled = false;
				lightPlayer.enabled = true;
				var result = await WaitForInputSequence(new[]
				{
					new ActionData
					{
						left = LegPoseCode.LiftUp,
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
				});
				if (result)
				{
					boy.SetTrigger("Success");
					await MainThreadTimerManager.Await(2);
					break;
				}
				lightBoy.enabled = false;
				boy.SetTrigger("Wrong");
				await MainThreadTimerManager.Await(2);
			}
			lightBoy.enabled = false;
			lightPlayer.enabled = false;
			GameRoot.Sunlight.ResetIntensity(0.2f);
			GameRoot.Player.InputBlock = default;
		}
		/// <summary>
		///     等待玩家输入指定动作序列
		/// </summary>
		/// <param name="sequence">要求的动作序列</param>
		/// <returns>是否成功完成序列</returns>
		Utilities.Awaitable<bool> WaitForInputSequence(ActionData[] sequence)
		{
			var awaitable = Utilities.Awaitable<bool>.Create(out var handle);

			// 使用闭包来避免污染类代码
			{
				var recordedActions = new List<ActionData>();
				var isActive = false;
				var hasStarted = false;
				var startTime = Time.time;
				var lastActionTime = Time.time;
				var timeThreshold = 0.1f; // 时间容错阈值
				var maxWaitTime = 10f; // 最大等待时间

				// 输入事件处理
				Action onAnyInput = null;
				Action onJump = null;
				onAnyInput = () =>
				{
					if (GameRoot.Player.HandIkInput.Jumping) return;
					RecordAction();
				};
				onJump = () => RecordAction();
				void RecordAction()
				{
					if (!isActive) return;

					// 标记已开始
					if (!hasStarted)
					{
						hasStarted = true;
						Debug.Log("踩水游戏: 检测开始 - 第一次输入");
					}

					// 更新上一个动作的结束时间
					if (recordedActions.Count > 0)
					{
						var last = recordedActions[^1];
						last.endTime = Time.time;
						recordedActions[^1] = last;
					}

					// 记录新动作
					var action = new ActionData
					{
						startTime = Time.time,
						left = GameRoot.Player.HandIkInput.LeftLeg,
						right = GameRoot.Player.HandIkInput.RightLeg,
						jumping = GameRoot.Player.HandIkInput.Jumping,
					};
					recordedActions.Add(action);
					lastActionTime = Time.time;
					Debug.Log($"踩水游戏: 记录动作 - 左:{action.left} 右:{action.right} 跳跃:{action.jumping}");

					// 检查结果
					CheckResult();
				}
				void CheckResult()
				{
					if (sequence.Length == 0) return;

					// 从后往前匹配，找到最长的匹配序列
					var matchCount = 0;
					var requiredIndex = sequence.Length - 1;
					var recordedIndex = recordedActions.Count - 1;
					while (requiredIndex >= 0 && recordedIndex >= 0)
					{
						var requiredAction = sequence[requiredIndex];
						var recordedAction = recordedActions[recordedIndex];

						// 检查动作是否匹配
						if (ActionData.Match(requiredAction, recordedAction))
						{
							matchCount++;
							requiredIndex--;
							recordedIndex--;
						}
						// 检查时间阈值 - 如果动作持续时间太短，跳过
						else if (recordedAction.endTime - recordedAction.startTime < timeThreshold)
						{
							recordedIndex--;
						}
						else
						{
							break;
						}
					}

					// 判断结果
					if (matchCount == sequence.Length)
					{
						// 完全匹配 - 成功
						Debug.Log($"踩水游戏: 检测成功! 匹配了 {matchCount}/{sequence.Length} 个动作");
						CleanupAndComplete(true);
					}
					else if (recordedActions.Count >= sequence.Length * 2)
					{
						// 动作数量过多 - 失败
						Debug.Log($"踩水游戏: 检测失败! 动作过多 ({recordedActions.Count} > {sequence.Length * 2})");
						CleanupAndComplete(false);
					}
				}
				void CheckTimeout()
				{
					if (!isActive) return;

					// 检查超时
					if (Time.time - lastActionTime > maxWaitTime)
					{
						Debug.Log($"踩水游戏: 检测超时 ({maxWaitTime}秒)");
						CleanupAndComplete(false);
					}
				}
				void CleanupAndComplete(bool success)
				{
					// 取消注册输入事件
					GameRoot.Player.HandIkInput.OnLeftLegChanged -= onAnyInput;
					GameRoot.Player.HandIkInput.OnRightLegChanged -= onAnyInput;
					GameRoot.Player.HandIkInput.OnJump -= onJump;
					GameRoot.Player.HandIkInput.OnLanded -= onAnyInput;
					isActive = false;
					handle.Set(success);
				}

				// 开始检测
				recordedActions.Clear();
				isActive = true;
				hasStarted = false;
				startTime = Time.time;
				lastActionTime = Time.time;

				// 注册输入事件
				GameRoot.Player.HandIkInput.OnLeftLegChanged += onAnyInput;
				GameRoot.Player.HandIkInput.OnRightLegChanged += onAnyInput;
				GameRoot.Player.HandIkInput.OnJump += onJump;
				GameRoot.Player.HandIkInput.OnLanded += onAnyInput;
				Debug.Log($"踩水游戏: 开始检测 {sequence.Length} 个动作的序列");

				// 启动超时检查协程
				StartCoroutine(TimeoutCheckCoroutine());
				System.Collections.IEnumerator TimeoutCheckCoroutine()
				{
					while (isActive)
					{
						CheckTimeout();
						yield return null;
					}
				}
			}
			return awaitable;
		}
	}
}
