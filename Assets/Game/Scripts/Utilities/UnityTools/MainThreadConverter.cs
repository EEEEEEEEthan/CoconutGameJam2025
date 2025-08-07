using System;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Game.Utilities.UnityTools
{
	public static class MainThreadConverter
	{
		internal sealed class Updater : MonoBehaviour
		{
			void Update() => converter.Update();
		}
		static readonly ThreadConverter converter = new();
		public static void InvokeAtNextUnityFrame(Action action) => converter.Invoke(action);
		public static ThreadConverter.Awaitable Await() => converter.Await();
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void Initialize()
		{
			var gameObject = new GameObject($"{nameof(MainThreadConverter)}");
			gameObject.AddComponent<Updater>();
			Object.DontDestroyOnLoad(gameObject);
		}
	}
}
