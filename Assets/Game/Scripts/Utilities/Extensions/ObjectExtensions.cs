using Newtonsoft.Json;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static string ToJson(this object @this, bool indented = true) =>
			JsonConvert.SerializeObject(@this,
				indented ? Formatting.Indented : Formatting.None,
				Vector2IntConverter.instance,
				Vector2Converter.instance
			);
		public static void Destroy(this Object @this)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Object.DestroyImmediate(@this);
				return;
			}
#endif
			Object.Destroy(@this);
		}
		public static T Instantiate<T>(this T @this, Transform parent) where T : Object => Object.Instantiate(@this, parent);
		public static T Instantiate<T>(this T @this, Vector3 localPosition, Quaternion localRotation, Transform parent) where T : Object =>
			Object.Instantiate(@this, localPosition, localRotation, parent);
	}
}
