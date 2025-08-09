using System.Collections.Generic;
using UnityEngine;
namespace Game.Gameplay.WaterGame
{
	public class WaterGameLevel : MonoBehaviour
	{
		[SerializeField] ActionData[] requiredActionSequence;
		public IReadOnlyList<ActionData> RequiredActionSequence => requiredActionSequence;
	}
}
