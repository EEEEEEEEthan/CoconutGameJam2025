using System;
using Game.Utilities;
using UnityEngine;
namespace Game.PaintEffect
{
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	public class Viewport : MonoBehaviour
	{
		[SerializeField] Camera targetCamera;
		[NonSerialized] RenderTexture renderTexture;
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
				}
			}
		}
	}
}
