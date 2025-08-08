using System;
using UnityEngine;
namespace Game.Gameplay
{
	public class GameBehaviour : MonoBehaviour
	{
		[SerializeField, HideInInspector,] GameRoot gameRoot;
		public GameRoot Root => gameRoot ??= GetComponentInParent<GameRoot>();
	}
}
