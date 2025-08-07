using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Game.Utilities
{
#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoad]
#endif
	public static class ApplicationEventListener
	{
		sealed class Listener : MonoBehaviour
		{
			void Update()
			{
				var currentScreenSize = new Vector2(Screen.width, Screen.height);
				if (currentScreenSize != screenSize)
				{
					screenSize = currentScreenSize;
					OnScreenSizeChanged?.TryInvoke();
				}
				OnUpdate?.TryInvoke();
			}
			void LateUpdate() => OnLateUpdate?.TryInvoke();
			void OnApplicationFocus(bool hasFocus) =>
				//Debug.Log($"OnApplicationFocus: {hasFocus}");
				ApplicationEventListener.OnApplicationFocus?.TryInvoke(hasFocus);
			void OnApplicationPause(bool pauseStatus)
			{
				Debug.Log($"OnApplicationPause: {pauseStatus}");
				Paused = pauseStatus;
				ApplicationEventListener.OnApplicationPause?.TryInvoke(pauseStatus);
			}
			void OnApplicationQuit()
			{
				Debug.Log("OnApplicationQuit");
				Playing = false;
				ApplicationEventListener.OnApplicationQuit?.TryInvoke();
			}
		}
		static Vector2 screenSize;
		public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == MainThreadId;
		public static bool Playing { get; private set; }
		public static bool Paused { get; private set; }
		public static int MainThreadId { get; private set; }
		public static event Action OnUpdate;
		public static event Action OnLateUpdate;
		public static event Action OnApplicationQuit;
		public static event Action<bool> OnApplicationFocus;
		public static event Action<bool> OnApplicationPause;
		public static event Action OnScreenSizeChanged;
		public static event Action OnEditorUpdate
		{
#if UNITY_EDITOR
			add => onEditorUpdate += value;
			remove => onEditorUpdate -= value;
#else
			add { }
			remove { }
#endif
		}

		// ReSharper disable once InconsistentNaming
		static event Action onEditorUpdate;
		static ApplicationEventListener()
		{
			MainThreadId = Thread.CurrentThread.ManagedThreadId;
#if UNITY_EDITOR
			UnityEditor.EditorApplication.update += static () => onEditorUpdate?.TryInvoke();
#endif
			TaskScheduler.UnobservedTaskException += static (_, e) => { Debug.LogException(e.Exception); };
			AppDomain.CurrentDomain.UnhandledException += static (_, e) => { Debug.LogException(e.ExceptionObject as Exception); };
		}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		static void AfterAssembliesLoaded()
		{
			MainThreadId = Thread.CurrentThread.ManagedThreadId;
			Playing = true;
		}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void AfterSceneLoad()
		{
			var gameObject = new GameObject($"{nameof(ApplicationEventListener)}");
			gameObject.AddComponent<Listener>();
			Object.DontDestroyOnLoad(gameObject);
		}
	}
}
