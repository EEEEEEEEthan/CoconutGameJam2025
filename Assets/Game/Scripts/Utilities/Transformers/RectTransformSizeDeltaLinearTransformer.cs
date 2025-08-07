using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	[RequireComponent(typeof(RectTransform))]
	sealed class RectTransformSizeDeltaLinearTransformer : LinearVector2Transformer
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
		public static void LinearSetSizeDelta(this RectTransform @this, Vector2 size, float duration, Action callback)
		{
			var transformer = @this.gameObject.GetOrAddComponent<RectTransformSizeDeltaLinearTransformer>();
			transformer.SetValue(size, duration, callback);
		}
	}
}
