using System;
using UnityEngine;
namespace Game.Utilities.Transformers
{
	sealed class LocalPositionSmoothTransformer : SmoothVector3Transformer
	{
		protected override Vector3 Value
		{
			get => transform.localPosition;
			set
			{
				if (float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z))
				{
					Debug.LogError($"Invalid local position value: {value}");
					return;
				}
				transform.localPosition = value;
			}
		}
	}
	static partial class Extensions
	{
		public static void SmoothMoveTo(
			this Transform transform,
			Vector3 localPosition,
			float smoothTime,
			Action callback = null)
		{
			var transformer = transform.gameObject.GetOrAddComponent<LocalPositionSmoothTransformer>();
			transformer.SetValue(localPosition, smoothTime, callback);
		}
	}
}
