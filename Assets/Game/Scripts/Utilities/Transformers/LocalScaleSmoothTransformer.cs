using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	sealed class LocalScaleSmoothTransformer : SmoothVector3Transformer
	{
		protected override Vector3 Value
		{
			get => transform.localScale;
			set => transform.localScale = value;
		}
	}
	static partial class Extensions
	{
		public static void SmoothScale(this Transform transform, Vector3 scale, float smoothTime, Action callback)
		{
			var transformer = transform.gameObject.GetOrAddComponent<LocalScaleSmoothTransformer>();
			transformer.SetValue(scale, smoothTime, callback);
		}
		public static Awaitable SmoothScale(this Transform transform, Vector3 scale, float smoothTime)
		{
			var awaitable = Awaitable.Create(out var handle);
			SmoothScale(transform, scale, smoothTime, handle.Set);
			return awaitable;
		}
	}
}
