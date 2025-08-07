using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	[RequireComponent(typeof(RectTransform))]
	sealed class RectTransformSizeDeltaSmoothTransformer : SmoothVector2Transformer
	{
		RectTransform cachedRectTransform;
		protected override Vector2 Value
		{
			get => RectTransform.sizeDelta;
			set => RectTransform.sizeDelta = value;
		}
		RectTransform RectTransform => cachedRectTransform ? cachedRectTransform : cachedRectTransform = GetComponent<RectTransform>();
	}
	public static partial class Extensions
	{
		public static void SmoothSetSizeDelta(this RectTransform @this, Vector2 size, float smoothTime, Action callback)
		{
			var transformer = @this.gameObject.GetOrAddComponent<RectTransformSizeDeltaSmoothTransformer>();
			transformer.SetValue(size, smoothTime, callback);
		}
		public static Awaitable SmoothSetSizeDelta(this RectTransform @this, Vector2 size, float smoothTime)
		{
			var awaitable = Awaitable.Create(out var handle);
			SmoothSetSizeDelta(@this, size, smoothTime, handle.Set);
			return awaitable;
		}
	}
}
