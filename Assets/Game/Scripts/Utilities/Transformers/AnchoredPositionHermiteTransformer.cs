using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	[RequireComponent(typeof(RectTransform))]
	sealed class AnchoredPositionHermiteTransformer : HermiteVector2Transformer
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
		public static void HermiteSetAnchoredPosition(
			this RectTransform @this,
			Vector2 position,
			float smoothTime,
			Action callback)
		{
			var transformer = @this.gameObject.GetOrAddComponent<AnchoredPositionHermiteTransformer>();
			transformer.SetValue(position, smoothTime, callback);
		}
		public static Awaitable HermiteSetAnchoredPosition(
			this RectTransform @this,
			Vector2 position,
			float smoothTime)
		{
			var awaitable = Awaitable.Create(out var handle);
			HermiteSetAnchoredPosition(@this, position, smoothTime, handle.Set);
			return awaitable;
		}
	}
}
