using System;
using System.Collections.Generic;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class WaterGameJudge : GameBehaviour
	{
		[SerializeField] List<ActionData> actions = new();
		readonly Action onAnyInput;
		public event Action<WaterGameLevel> OnLevelCompleted;
		WaterGameJudge() =>
			onAnyInput = () =>
			{
				if (actions.Count > 0)
				{
					var last = actions[^1];
					last.endTime = Time.time;
					actions[^1] = last;
				}
				var action = new ActionData
				{
					startTime = Time.time,
					left = GameRoot.Player.HandIkInput.LeftLeg,
					right = GameRoot.Player.HandIkInput.RightLeg,
					jumping = GameRoot.Player.HandIkInput.Jumping,
				};
				actions.Add(action);
				Check();
			};
		void OnDisable() => actions.Clear();
		void OnTriggerEnter(Collider other)
		{
			if (other != GameRoot.Player.HandIkInput.LeftGroundDetect.Collider) return;
			enabled = true;
			GameRoot.Player.HandIkInput.OnLeftLegChanged += onAnyInput;
			GameRoot.Player.HandIkInput.OnRightLegChanged += onAnyInput;
			GameRoot.Player.HandIkInput.OnJump += onAnyInput;
		}
		void OnTriggerExit(Collider other)
		{
			if (other != GameRoot.Player.HandIkInput.LeftGroundDetect.Collider) return;
			enabled = false;
			GameRoot.Player.HandIkInput.OnLeftLegChanged -= onAnyInput;
			GameRoot.Player.HandIkInput.OnRightLegChanged -= onAnyInput;
			GameRoot.Player.HandIkInput.OnJump -= onAnyInput;
		}
		void Check()
		{
			var level = GetComponentInChildren<WaterGameLevel>();
			if (level == null) return;
			var i = 0;
			var j = 0;
			while (true)
			{
				if (i >= level.RequiredActionSequence.Count)
				{
					level.gameObject.SetActive(false);
					Debug.Log($"Water game level completed: {level.name}", level);
					OnLevelCompleted?.TryInvoke(level);
					return;
				}
				if (j >= actions.Count) break;
				var requiredAction = level.RequiredActionSequence[^(i + 1)];
				var action = actions[^(i + 1)];
				if (ActionData.Match(requiredAction, action))
				{
					++i;
					++j;
				}
				else if (action.endTime - action.startTime < 0.1f)
				{
					++j;
				}
				else
				{
					break;
				}
			}
		}
	}
}
