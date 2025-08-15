using System;
using Game.FingerRigging;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	[Serializable]
	public struct ActionData
	{
		public static bool Match(ActionData a, ActionData b) => a.left == b.left && a.right == b.right && a.jumping == b.jumping;
		public ActionData(char left, char right)
		{
			this.left = left switch
			{
				'Q' => LegPoseCode.LiftForward,
				'A' => LegPoseCode.LiftUp,
				'Z' => LegPoseCode.LiftBackward,
				_ => LegPoseCode.Idle,
			};
			this.right = right switch
			{
				'W' => LegPoseCode.LiftForward,
				'S' => LegPoseCode.LiftUp,
				'X' => LegPoseCode.LiftBackward,
				_ => LegPoseCode.Idle,
			};
			jumping = false;
			startTime = 0f;
			endTime = 0f;
		}
		[HideInInspector] public float startTime;
		[HideInInspector] public float endTime;
		public LegPoseCode left;
		public LegPoseCode right;
		public bool jumping;
		public override string ToString()
		{
			using (StringBuilderPoolThreaded.Rent(out var builder))
			{
				builder.Append("(");
				switch (left)
				{
					case LegPoseCode.Idle:
						builder.Append('O');
						break;
					case LegPoseCode.LiftForward:
						builder.Append('Q');
						break;
					case LegPoseCode.LiftUp:
						builder.Append('A');
						break;
					case LegPoseCode.LiftBackward:
						builder.Append('Z');
						break;
				}
				builder.Append(',');
				switch (right)
				{
					case LegPoseCode.Idle:
						builder.Append('O');
						break;
					case LegPoseCode.LiftForward:
						builder.Append('W');
						break;
					case LegPoseCode.LiftUp:
						builder.Append('S');
						break;
					case LegPoseCode.LiftBackward:
						builder.Append('X');
						break;
				}
				builder.Append(')');
				return builder.ToString();
			}
		}
	}
}
