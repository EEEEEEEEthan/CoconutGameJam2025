using System.Collections.Generic;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	[CreateAssetMenu(fileName = nameof(WaterGameLevel), menuName = "Assets/*Game*/" + nameof(WaterGameLevel) + "(踩水游戏关卡)", order = 1)]
	public class WaterGameLevel : ScriptableObject
	{
		[SerializeField] ActionData[] requiredActionSequence;
		public IReadOnlyList<ActionData> RequiredActionSequence => requiredActionSequence;
	}
}
