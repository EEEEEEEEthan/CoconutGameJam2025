using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	[RequireComponent(typeof(RectTransform))]
	sealed class AnchoredPositionSmoothTransformer : SmoothVector2Transformer
	{
		RectTransform cachedRectTransform;
		protected override Vector2 Value
		{
			get => RectTransform.anchoredPosition;
			set => RectTransform.anchoredPosition = value;
		}
		RectTransform RectTransform => cachedRectTransform ? cachedRectTransform : cachedRectTransform = GetComponent<RectTransform>();
	}
	public static partial class Extensions
	{
		public static void SmoothSetAnchoredPosition(this RectTransform @this, Vector2 position, float smoothTime, Action callback)
		{
			var transformer = @this.gameObject.GetOrAddComponent<AnchoredPositionSmoothTransformer>();
			transformer.SetValue(position, smoothTime, callback);
		}
	}
}
