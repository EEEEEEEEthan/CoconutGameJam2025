using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Utilities.Pools;
using UnityEditor;
using UnityEngine;
namespace Game.Utilities.UnityTools
{
	public interface IUnityRefreshableItem
	{
		void OnRefresh();
	}
	public class Refreshable<T> : IUnityRefreshableItem
	{
		static readonly IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
		public static implicit operator T(Refreshable<T> refreshable) => refreshable.Value;
		readonly Action<T> onRefresh;
		T value;
		public T Value
		{
			get => value;
			set
			{
				if (comparer.Equals(this.value, value)) return;
				this.value = value;
				this.Refresh();
			}
		}
		public Refreshable(Action<T> onRefresh)
		{
			this.onRefresh = onRefresh;
			value = default;
		}
		public Refreshable(T defaultValue, Action<T> onRefresh)
		{
			this.onRefresh = onRefresh;
			value = defaultValue;
		}
		void IUnityRefreshableItem.OnRefresh() => onRefresh?.TryInvoke(value);
	}
	public class Refreshable : IUnityRefreshableItem
	{
		readonly Action onRefresh;
		public Refreshable(Action onRefresh) => this.onRefresh = onRefresh;
		void IUnityRefreshableItem.OnRefresh() => onRefresh();
	}
	sealed class RefreshableItemCollection
	{
		readonly HashSet<IUnityRefreshableItem> dirtyItems = new();
		public void Update()
		{
			Pooled disposable;
			IUnityRefreshableItem[] array;
			lock (dirtyItems)
			{
				disposable = ArrayPoolThreaded<IUnityRefreshableItem>.RentWithoutDefaultValue(dirtyItems.Count, out array);
				dirtyItems.CopyTo(array, 0);
				dirtyItems.Clear();
			}
			using (disposable)
			{
				var length = array.Length;
				for (var i = 0; i < length; ++i)
				{
					var item = array[i];
					try
					{
						item.OnRefresh();
					}
					catch (Exception e)
					{
						Debug.LogException(e);
					}
				}
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MarkDirty(IUnityRefreshableItem item, bool immediate)
		{
			if (immediate)
			{
				lock (dirtyItems)
				{
					dirtyItems.Remove(item);
				}
				try
				{
					item.OnRefresh();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
			else
			{
				lock (dirtyItems)
				{
					dirtyItems.Add(item);
				}
			}
		}
	}
	public static class MainThreadRefreshableExtensions
	{
		static readonly RefreshableItemCollection runtimeCollection = new();
#if UNITY_EDITOR
		static readonly RefreshableItemCollection editorCollection = new();
#endif
		static MainThreadRefreshableExtensions()
		{
#if UNITY_EDITOR
			EditorApplication.update += editorCollection.Update;
#endif
			ApplicationEventListener.OnLateUpdate += runtimeCollection.Update;
		}
		/// <summary>
		///     多线程安全，但实际刷新会在unity主线程
		/// </summary>
		/// <param name="this"></param>
		/// <param name="immediate"></param>
		public static void Refresh(this IUnityRefreshableItem @this, bool immediate = false)
		{
			if (@this is null) return;
			if (ApplicationEventListener.Playing)
				runtimeCollection.MarkDirty(@this, immediate);
#if UNITY_EDITOR
			else
				editorCollection.MarkDirty(@this, immediate);
#endif
		}
	}
}
