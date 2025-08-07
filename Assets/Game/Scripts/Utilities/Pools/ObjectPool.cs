using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Game.Utilities.Time;
using Game.Utilities.UnityTools;
using UnityEngine;
namespace Game.Utilities.Pools
{
	public struct Pooled : IDisposable
	{
		ObjectPool pool;
		object item;
		internal Pooled(ObjectPool pool, object item)
		{
			this.pool = pool;
			this.item = item;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (pool != null)
			{
				pool.Return(item);
				pool = null;
				item = null;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IDisposable.Dispose() => Dispose();
	}
	public class ObjectPool
	{
		readonly Action onTimesUp;
		readonly Func<object> create;
		readonly Func<object, bool> onRent;
		readonly Func<object, bool> onReturn;
		readonly Action<object> onDispose;
		readonly Stack stack = new();
		TimerId timer;
		public ObjectPool(Func<object> create, Func<object, bool> onRent = null, Func<object, bool> onReturn = null, Action<object> onDispose = null)
		{
			this.create = create ?? throw new ArgumentNullException(nameof(create));
			this.onRent = onRent;
			this.onReturn = onReturn;
			this.onDispose = onDispose;
			onTimesUp = OnTimesUp;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Pooled Rent(out object item)
		{
			while (TryPop(out item))
				if (OnRent(item))
					return new(this, item);
			item = create();
			OnRent(item);
			return new(this, item);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Return(object item)
		{
			if (OnReturn(item))
			{
				Push(item);
				if (!timer.Valid) StartTimer();
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool OnRent(object item)
		{
			if (onRent is null) return true;
			try
			{
				if (onRent(item)) return true;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool OnReturn(object item)
		{
			if (onReturn is null) return true;
			try
			{
				if (onReturn(item)) return true;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void OnTimesUp()
		{
			var count = stack.Count;
			var needToFree = (count * 0.1f).CeilToInt();
			for (var i = 0; i < needToFree; ++i)
				if (TryPop(out var item))
					onDispose?.TryInvoke(item);
			lock (stack)
			{
				if (stack.Count <= 0)
				{
					timer = default;
					return;
				}
			}
			StartTimer();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void StartTimer() => timer = MainThreadTimerManager.InvokeAfter(1, onTimesUp);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Push(object item)
		{
			lock (stack)
			{
				stack.Push(item);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool TryPop(out object item)
		{
			lock (stack)
			{
				if (stack.Count > 0)
				{
					item = stack.Pop();
					return true;
				}
				item = null;
				return false;
			}
		}
	}
	public class ObjectPool<T> : ObjectPool where T : class
	{
		public ObjectPool(Func<T> create, Func<T, bool> onRent = null, Func<T, bool> onReturn = null, Action<T> onDispose = null) :
			base(
				create: create,
				onRent: onRent is null ? null : obj => onRent((T)obj),
				onReturn: onReturn is null ? null : obj => onReturn((T)obj),
				onDispose: onDispose is null ? null : obj => onDispose((T)obj)) { }
		public Pooled Rent(out T item)
		{
			var pooled = base.Rent(out var obj);
			item = (T)obj;
			return pooled;
		}
	}
}
