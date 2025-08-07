using System;
using System.Collections.Generic;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Utilities
{
	sealed class GUIDebugDrawer : MonoBehaviour
	{
		static readonly List<GUIDebugDrawer> windows = new();
		public static bool PointerOverWindow
		{
			get
			{
				var position = Input.mousePosition;
				position.y = Screen.height - position.y;
				for (var i = windows.Count; i-- > 0;)
					if (windows[i].rect.Contains(position))
						return true;
				return false;
			}
		}
		public string title = "title";
		public Rect rect;
		public int windowId;
		public Action<Action> guiCallback;
		void OnEnable() => windows.Add(this);
		void OnDisable() => windows.Remove(this);
		void OnGUI() => rect = GUILayout.Window(windowId, rect, DrawWindow, title);
		void DrawWindow(int windowID)
		{
			guiCallback?.Invoke(Close);
			GUI.DragWindow();
		}
		void Close() => Destroy(gameObject);
	}
	public static class GUIDebug
	{
		static int windowId;
		public static bool PointerOverWindow => GUIDebugDrawer.PointerOverWindow;
		public static async void AsyncCreateWindow(string title, Vector2 position, Action<Action> onGUI)
		{
			try
			{
				await MainThreadConverter.Await();
				GameObject gameObject = new();
				var drawer = gameObject.AddComponent<GUIDebugDrawer>();
				drawer.title = title;
				drawer.rect = new(position, default);
				drawer.windowId = ++windowId;
				drawer.guiCallback = onGUI;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		public static void CreateWindow(string title, Action<Action> onGUI)
		{
			var position = Input.mousePosition;
			position.y = Screen.height - position.y;
			AsyncCreateWindow(title, position, onGUI);
		}
	}
}
