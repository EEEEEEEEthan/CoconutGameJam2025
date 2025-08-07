using System;
using Game.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Game.PaintEffect
{
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(RawImage))]
	public class Viewport : MonoBehaviour
	{
#if UNITY_EDITOR
		[CustomEditor(typeof(Viewport)), CanEditMultipleObjects,]
		class Editor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();
				// draw renderTexture
				if (targets.Length == 1)
				{
					var target = (Viewport)this.target;
					if (target.renderTexture != null)
					{
						var rect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(false));
						EditorGUI.DrawPreviewTexture(rect, target.renderTexture);
					}
				}
			}
		}
#endif
		[SerializeField] Camera targetCamera;
		[NonSerialized] RenderTexture renderTexture;
		RawImage rawImage;
		void Update()
		{
			if (!targetCamera)
			{
				if (renderTexture)
				{
					RenderTexture.ReleaseTemporary(renderTexture);
					renderTexture = null;
				}
			}
			else
			{
				var rectTransform = (RectTransform)transform;
				var rect = rectTransform.rect;
				var width = rect.width.CeilToInt();
				var height = rect.height.CeilToInt();
				if (renderTexture)
					if (renderTexture.width != width || renderTexture.height != height)
					{
						RenderTexture.ReleaseTemporary(renderTexture);
						renderTexture = null;
					}
				if (!renderTexture)
				{
					renderTexture = RenderTexture.GetTemporary(width, height);
					targetCamera.targetTexture = renderTexture;
					(rawImage ??= GetComponent<RawImage>()).texture = renderTexture;
				}
			}
		}
	}
}
