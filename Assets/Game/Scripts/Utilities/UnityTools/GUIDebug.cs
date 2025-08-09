using System;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Utilities.UnityTools
{
	public static class GUIDebug
	{
		public interface IWindow
		{
			public Vector2 Size { get; set; }
			public void Close();
		}
		sealed class GUIDebugDrawer : MonoBehaviour, IWindow
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
			public Action<IWindow> onGUI;
			public Vector2 Size
			{
				get => rect.size;
				set => rect.size = value;
			}
			void OnEnable() => windows.Add(this);
			void OnDisable() => windows.Remove(this);
			void OnGUI()
			{
				var matrix = GUI.matrix;
				var widthFactor = Screen.width / 1280f * 2;
				var heightFactor = Screen.height / 1280f * 2;
				var scaleFactor = Mathf.Min(widthFactor, heightFactor);
				var scale = new Vector3(scaleFactor, scaleFactor, 1);
				GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
				rect = GUILayout.Window(windowId, rect, DrawWindow, title);
				GUI.matrix = matrix;
			}
			public void Close() => Destroy(gameObject);
			void DrawWindow(int windowID)
			{
				onGUI?.Invoke(this);
				GUI.DragWindow();
			}
		}
		static int windowId;
		public static bool PointerOverWindow => GUIDebugDrawer.PointerOverWindow;
		public static IWindow CreateWindow(string title, Rect rect, Action<IWindow> onGUI)
		{
			var gameObject = new GameObject(nameof(GUIDebugDrawer));
			var drawer = gameObject.AddComponent<GUIDebugDrawer>();
			drawer.title = title;
			drawer.rect = rect;
			drawer.windowId = ++windowId;
			drawer.onGUI = onGUI;
			return drawer;
		}
	}
}
