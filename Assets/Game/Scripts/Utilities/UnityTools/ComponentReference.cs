using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Utilities.UnityTools
{
	[Obsolete("use [ObjectReference] instead")]
	public class ComponentReference<TComponent>
	{
		public readonly string path;
		public readonly MonoBehaviour monoBehaviour;
		(TComponent boxed, Component unboxed) cachedComponent;
		Transform cachedTransform;
		[NotNull]
		public Transform Transform
		{
			get
			{
				if (cachedTransform) return cachedTransform;
				cachedTransform = monoBehaviour.transform.Find(path, true);
				if (!cachedTransform && Application.isPlaying)
					using (StringBuilderPoolThreaded.Rent(out var builder))
					{
						Debug.LogError(builder.Append("GameObject缺失:").Append(this), monoBehaviour);
					}
				return cachedTransform;
			}
		}
		[NotNull] public GameObject GameObject => Transform.gameObject;
		[NotNull]
		public TComponent Component
		{
			get
			{
				if (cachedComponent.unboxed) return cachedComponent.boxed;
				var component = Transform.GetComponent<TComponent>();
				if (component is null && Application.isPlaying)
				{
					Debug.LogError(new StringBuilder().Append("组件缺失:").Append(this), monoBehaviour);
					return default;
				}
				cachedComponent = (component, component as Component);
				return cachedComponent.boxed;
			}
		}
		public ComponentReference(MonoBehaviour monoBehaviour, string path)
		{
			this.path = path;
			this.monoBehaviour = monoBehaviour;
			cachedComponent = default;
			cachedTransform = null;
		}
		public override string ToString()
		{
			using (StringBuilderPoolThreaded.Rent(out var builder))
			{
				builder.Append("ComponentReference<").Append(typeof(TComponent).Name).Append(">(").Append(nameof(path)).Append(":").Append(path).Append(")");
				return builder.ToString();
			}
		}
		public void SetActive(bool active)
		{
			if (Transform) Transform.gameObject.SetActive(active);
		}
		public T GetComponent<T>() => Transform.GetComponent<T>();
	}
}
