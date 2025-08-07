using UnityEngine;
namespace Game.Utilities.Transformers
{
	sealed class LocalXSingleAngleSmoothSingleTransformer : SmoothSingleAngleTransformer
	{
		protected override float Value
		{
			get => transform.localEulerAngles.x;
			set
			{
				var transform = this.transform;
				var angles = transform.localEulerAngles;
				angles.x = value;
				transform.localEulerAngles = angles;
			}
		}
	}
	static partial class Extensions
	{
		public static void SmoothSetXAngle(this Transform transform, float angle, float smoothTime)
		{
			if (angle == transform.localEulerAngles.x) return;
			var transformer = transform.gameObject.GetOrAddComponent<LocalXSingleAngleSmoothSingleTransformer>();
			transformer.SetValue(angle, smoothTime);
		}
	}
}
