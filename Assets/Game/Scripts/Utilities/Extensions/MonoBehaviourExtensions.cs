using System.Collections;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static void StartCoroutine(this MonoBehaviour @this, ref Coroutine coroutine, IEnumerator routine)
		{
			@this.StopCoroutine(ref coroutine);
			coroutine = @this.StartCoroutine(routine);
		}
		public static void StopCoroutine(this MonoBehaviour @this, ref Coroutine coroutine)
		{
			if (coroutine != null)
			{
				@this.StopCoroutine(coroutine);
				coroutine = null;
			}
		}
	}
}
