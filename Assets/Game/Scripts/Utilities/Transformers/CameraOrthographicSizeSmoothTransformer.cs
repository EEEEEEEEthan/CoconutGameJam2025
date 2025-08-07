using UnityEngine;
namespace Game.Utilities.Transformers
{
	[RequireComponent(typeof(Camera))]
	[DisallowMultipleComponent]
	sealed class CameraOrthographicSizeSmoothTransformer : SmoothSingleTransformer
	{
		Camera targetCamera;
		protected override float Value
		{
			get => targetCamera.orthographicSize;
			set => targetCamera.orthographicSize = value;
		}
		void Awake() => targetCamera = GetComponent<Camera>();
	}
	public static partial class Extensions
	{
		public static void SmoothSetOrthographicSize(this Camera camera, float size, float smoothTime)
		{
			if (size == camera.orthographicSize) return;
			var transformer = camera.gameObject.GetOrAddComponent<CameraOrthographicSizeSmoothTransformer>();
			transformer.SetValue(size, smoothTime);
		}
	}
}
