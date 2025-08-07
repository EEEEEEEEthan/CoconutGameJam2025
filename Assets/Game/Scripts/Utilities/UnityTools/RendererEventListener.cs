using System;
using UnityEngine;
namespace Game.Utilities.UnityTools
{
	public sealed class RendererEventListener : MonoBehaviour
	{
		bool visible;
		public bool Visible
		{
			get => visible;
			set
			{
				if (visible == value) return;
				visible = value;
				OnVisibilityChanged?.TryInvoke();
			}
		}
		public event Action OnVisibilityChanged;
		void OnDestroy() => Visible = false;
		void OnBecameInvisible() => Visible = false;
		void OnBecameVisible() => Visible = true;
	}
}
