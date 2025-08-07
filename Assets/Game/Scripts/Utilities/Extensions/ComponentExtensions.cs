using System.Collections.Generic;
using Game.Utilities.Pools;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static T AddComponent<T>(this Component @this) where T : Component => @this.gameObject.AddComponent<T>();
		public static Pooled GetComponentsInChildren<T>(
			this Component @this,
			out List<T> components,
			bool includeInactive = false,
			bool recursive = true)
		{
			var disposable = ListPoolThreaded<T>.Rent(out components);
			if (recursive)
				@this.GetComponentsInChildren(includeInactive, components);
			else
				using (ListPoolThreaded<T>.Rent(out var buffer))
				{
					@this.GetComponents(buffer);
					components.AddRange(buffer);
					buffer.Clear();
					var childCount = @this.transform.childCount;
					if (includeInactive)
						for (var i = 0; i < childCount; i++)
						{
							var child = @this.transform.GetChild(i);
							child.GetComponents(buffer);
							buffer.Clear();
							components.AddRange(buffer);
						}
					else
						for (var i = 0; i < childCount; i++)
						{
							var child = @this.transform.GetChild(i);
							if (!child.gameObject.activeSelf) continue;
							child.GetComponents(buffer);
							buffer.Clear();
							components.AddRange(buffer);
						}
				}
			return disposable;
		}
		public static void SetParent(this Component @this, Component parent, bool worldPositionStays = false)
		{
			if (!@this) return;
			if (!parent) return;
			var thisTransform = @this.transform;
			if (thisTransform.parent == parent.transform) return;
			thisTransform.SetParent(parent.transform, worldPositionStays);
		}
		public static T GetOrAddComponent<T>(this Component @this) where T : Component => @this.gameObject.GetOrAddComponent<T>();
		public static void SetActive(this Component @this, bool value)
		{
			if (!@this)
			{
				Debug.LogError($"{nameof(@this)} is null");
				return;
			}
			var thisGameObject = @this.gameObject;
			if (thisGameObject.activeSelf == value) return;
			thisGameObject.SetActive(value);
		}
		public static bool TryGetComponentInChildren<T>(this Component @this, out T component)
		{
			if (!@this)
			{
				Debug.LogError($"{nameof(@this)} is null");
				component = default;
				return false;
			}
			component = @this.transform.GetComponentInChildren<T>();
			return component is { };
		}
		public static bool TryGetComponentInParent<T>(this Component @this, out T component)
		{
			if (!@this)
			{
				Debug.LogError($"{nameof(@this)} is null");
				component = default;
				return false;
			}
			component = @this.transform.GetComponentInParent<T>();
			return (Object)(object)component;
		}
	}
}
