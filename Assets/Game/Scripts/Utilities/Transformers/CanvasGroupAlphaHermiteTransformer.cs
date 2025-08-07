using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	[RequireComponent(typeof(CanvasGroup))]
	[DisallowMultipleComponent]
	sealed class CanvasGroupAlphaHermiteTransformer : HermiteSingleTransformer
	{
		CanvasGroup cachedCanvasGroup;
		protected override float Value
		{
			get => CanvasGroup.alpha;
			set => CanvasGroup.alpha = value;
		}
		CanvasGroup CanvasGroup => cachedCanvasGroup ? cachedCanvasGroup : cachedCanvasGroup = GetComponent<CanvasGroup>();
	}
	public static partial class Extensions
	{
		public static void HermiteSetAlpha(this CanvasGroup canvasGroup, float alpha, float smoothTime, Action callback)
		{
			var transformer = canvasGroup.gameObject.GetOrAddComponent<CanvasGroupAlphaHermiteTransformer>();
			transformer.SetValue(alpha, smoothTime, callback);
		}
		public static Awaitable HermiteSetAlpha(this CanvasGroup canvasGroup, float alpha, float smoothTime)
		{
			var awaitable = Awaitable.Create(out var handle);
			canvasGroup.HermiteSetAlpha(alpha, smoothTime, handle.Set);
			return awaitable;
		}
	}
}
