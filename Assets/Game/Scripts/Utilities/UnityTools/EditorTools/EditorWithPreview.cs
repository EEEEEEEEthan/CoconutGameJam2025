#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace Game.Utilities.UnityTools.EditorTools
{
	/// <summary>
	///     3D预览相关Editor的基类，提供拖拽、预览、缩略图功能
	/// </summary>
	public abstract class EditorWithPreview : UnityEditor.Editor
	{
		Vector2 previewDir = new(120f, 30f);
		float cameraDistance = 100f;
		PreviewRenderUtility previewRenderUtility;
		/// <summary>
		///     相机距离的最小值和最大值
		/// </summary>
		protected abstract (float min, float max) CameraDistanceRange { get; }
		protected void OnEnable()
		{
			previewRenderUtility = new();
			previewRenderUtility.camera.transform.position = new(0, 0, -6);
			previewRenderUtility.camera.transform.rotation = Quaternion.identity;
		}
		protected void OnDisable() => previewRenderUtility?.Cleanup();
		public override bool HasPreviewGUI()
		{
			return CanPreview();
		}
		/// <summary>
		///     Project窗口缩略图预览
		/// </summary>
		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			if (!CanPreview()) return null; 
			var rect = new Rect(0, 0, width, height);
			// 使用固定的角度和距离生成缩略图
			var renderTexture = RenderPreview(rect, new(120f, 30f), 100f);
			if (renderTexture == null) return null;

			// 将RenderTexture转换为Texture2D
			var previousActive = RenderTexture.active;
			RenderTexture.active = renderTexture;
			var texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
			texture2D.ReadPixels(new(0, 0, width, height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = previousActive;
			return texture2D;
		}
		/// <summary>
		///     预览窗口GUI，处理拖拽和缩放
		/// </summary>
		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			if (!CanPreview()) return;
			// 处理鼠标拖拽和滚轮缩放
			var controlID = GUIUtility.GetControlID(FocusType.Passive);
			var eventType = Event.current.GetTypeForControl(controlID);
			var range = CameraDistanceRange;
			switch (eventType)
			{
				case EventType.MouseDown:
					if (r.Contains(Event.current.mousePosition))
					{
						GUIUtility.hotControl = controlID;
						Event.current.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID)
					{
						previewDir += Event.current.delta;
						Event.current.Use();
						Repaint();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						GUIUtility.hotControl = 0;
						Event.current.Use();
					}
					break;
				case EventType.ScrollWheel:
					if (r.Contains(Event.current.mousePosition))
					{
						cameraDistance += Event.current.delta.y * 0.5f;
						cameraDistance = Mathf.Clamp(cameraDistance, range.min, range.max);
						Event.current.Use();
						Repaint();
					}
					break;
			}
			if (Event.current.type == EventType.Repaint)
			{
				var texture = RenderPreview(r, previewDir, cameraDistance);
				if (texture) GUI.DrawTexture(r, texture, ScaleMode.StretchToFill, false);
			}
		}
		/// <summary>
		///     检查是否可以进行预览
		/// </summary>
		protected abstract bool CanPreview();
		/// <summary>
		///     获取预览内容的边界，用于相机定位
		/// </summary>
		protected abstract Bounds GetPreviewBounds();
		/// <summary>
		///     绘制预览内容，子类完全控制渲染逻辑
		/// </summary>
		protected abstract void DrawPreviewContent(PreviewRenderUtility utility);
		/// <summary>
		///     共用的预览渲染方法
		/// </summary>
		RenderTexture RenderPreview(Rect rect, Vector2 dir, float distance)
		{
			if (!CanPreview()) return null;
			previewRenderUtility.BeginPreview(rect, GUIStyle.none);
			var bounds = GetPreviewBounds();
			var modelCenter = bounds.center;
			var rot = Quaternion.Euler(dir.y, dir.x, 0);
			var pos = modelCenter + rot * (Vector3.back * distance);
			previewRenderUtility.camera.farClipPlane = 1000;
			previewRenderUtility.camera.transform.position = pos;
			previewRenderUtility.camera.transform.LookAt(modelCenter, Vector3.up);
			DrawPreviewContent(previewRenderUtility);
			previewRenderUtility.camera.Render();
			return (RenderTexture)previewRenderUtility.EndPreview();
		}
	}
}
#endif
