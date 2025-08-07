using UnityEngine;
namespace Game.Utilities.Transformers
{
	sealed class LocalYSingleAngleSmoothSingleTransformer : SmoothSingleAngleTransformer
	{
		protected override float Value
		{
			get => transform.localEulerAngles.y;
			set
			{
				var transform = this.transform;
				var angles = transform.localEulerAngles;
				angles.y = value;
				transform.localEulerAngles = angles;
			}
		}
	}
	static partial class Extensions
	{
		public static void SmoothSetYAngle(this Transform transform, float angle, float smoothTime)
		{
			if (angle == transform.localEulerAngles.y) return;
			var transformer = transform.gameObject.GetOrAddComponent<LocalYSingleAngleSmoothSingleTransformer>();
			transformer.SetValue(angle, smoothTime);
		}
	}
}
