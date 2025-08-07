using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	sealed class LocalPositionSmoothTransformer : SmoothVector3Transformer
	{
		protected override Vector3 Value
		{
			get => transform.localPosition;
			set => transform.localPosition = value;
		}
	}
	static partial class Extensions
	{
		public static void SmoothMoveTo(
			this Transform transform,
			Vector3 position,
			float smoothTime,
			Action callback = null)
		{
			var transformer = transform.gameObject.GetOrAddComponent<LocalPositionSmoothTransformer>();
			transformer.SetValue(position, smoothTime, callback);
		}
	}
}
