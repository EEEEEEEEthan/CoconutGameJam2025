using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component)
		{
			component = gameObject.GetComponentInParent<T>();
			return component != null;
		}
		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
		{
			var component = gameObject.GetComponent<T>();
			if (!component) component = gameObject.AddComponent<T>();
			if (!component)
				Debug.LogError($"Failed to add component of type {typeof(T).Name} to GameObject {gameObject.name}");
			return component;
		}
		public static void MakeChildrenInternal(this GameObject @this)
		{
			var transform = @this.transform;
			for (var i = transform.childCount; i-- > 0;)
			{
				var child = transform.GetChild(i).gameObject;
				child.tag = "Internal";
				child.hideFlags |= HideFlags.HideInHierarchy;
			}
		}
		public static void Destroy(this GameObject gameObject, float delay)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Object.DestroyImmediate(gameObject);
				return;
			}
#endif
			Object.Destroy(gameObject, delay);
		}
		public static void DestroyImmediate(this GameObject gameObject) => Object.DestroyImmediate(gameObject);
	}
}
