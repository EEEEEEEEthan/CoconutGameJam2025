using System;
using Game.Utilities;
using UnityEngine;
namespace Game.Gameplay
{
	public static class Extensions
	{
		class JumpBehaviour : MonoBehaviour
		{
			public void Jump(Vector3 target, float height, float duration, Action callback)
			{
				this.enabled = true;
			}
			void Update()
			{
				// if (end)
				// {
				//    this.enabled = false;
				//    callback?.Invoke();
				//    return;
				// }
			}
		}
		public static void Jump(this Transform @this, Vector3 target, float height, float duration, Action callback)
		{
			var jumpBehaviour = @this.GetOrAddComponent<JumpBehaviour>();
			jumpBehaviour.Jump(target, height, duration, callback);
		}
	}
}
