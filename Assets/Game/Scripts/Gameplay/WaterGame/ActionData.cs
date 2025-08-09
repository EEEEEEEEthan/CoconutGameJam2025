using System;
using Game.FingerRigging;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	[Serializable]
	public struct ActionData
	{
		public static bool Match(ActionData a, ActionData b) => a.left == b.left && a.right == b.right && a.jumping == b.jumping;
		[HideInInspector] public float startTime;
		[HideInInspector] public float endTime;
		public LegPoseCode left;
		public LegPoseCode right;
		public bool jumping;
	}
}
