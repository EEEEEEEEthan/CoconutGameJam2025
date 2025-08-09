using System;
using System.Collections.Generic;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class WaterGameJudge : GameBehaviour
	{
		[SerializeField] List<WaterGameLevel> levels = new();
		[SerializeField, HideInInspector,] List<ActionData> actions = new();
		readonly Action onAnyInput;
		readonly Action onJump;
		public event Action<WaterGameLevel> OnLevelCompleted;
		WaterGameJudge()
		{
			onAnyInput = () =>
			{
				if (GameRoot.Player.HandIkInput.Jumping) return;
				RecordAction();
			};
			onJump = RecordAction;
		}
		void OnDisable() => actions.Clear();
		void OnTriggerEnter(Collider other)
		{
			if (other != GameRoot.Player.HandIkInput.LeftGroundDetect.Collider) return;
			enabled = true;
			GameRoot.Player.HandIkInput.OnLeftLegChanged += onAnyInput;
			GameRoot.Player.HandIkInput.OnRightLegChanged += onAnyInput;
			GameRoot.Player.HandIkInput.OnJump += onJump;
			GameRoot.Player.HandIkInput.OnLanded += onAnyInput;
		}
		void OnTriggerExit(Collider other)
		{
			if (other != GameRoot.Player.HandIkInput.LeftGroundDetect.Collider) return;
			enabled = false;
			GameRoot.Player.HandIkInput.OnLeftLegChanged -= onAnyInput;
			GameRoot.Player.HandIkInput.OnRightLegChanged -= onAnyInput;
			GameRoot.Player.HandIkInput.OnJump -= onJump;
			GameRoot.Player.HandIkInput.OnLanded -= onAnyInput;
		}
		void RecordAction()
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
		}
		void Check()
		{
			if (levels.Count <= 0) return;
			var level = levels[0];
			var i = 0;
			var j = 0;
			while (true)
			{
				if (i >= level.RequiredActionSequence.Count)
				{
					levels.RemoveAt(0);
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
