using System;
using Game.Utilities.Time;
using UnityEngine;
namespace Game.Utilities.UnityTools
{
	/// <summary>
	///     多线程安全
	/// </summary>
	public static class MainThreadTimerManager
	{
		static readonly Timer timer;
		static MainThreadTimerManager()
		{
			timer = new();
			ApplicationEventListener.OnUpdate -= update;
			ApplicationEventListener.OnUpdate += update;
			return;
			static void update()
			{
#if UNITY_EDITOR
				if (!Application.isPlaying) return;
#endif
				timer.Update(UnityEngine.Time.realtimeSinceStartupAsDouble);
			}
		}
		/// <summary>
		///     多线程安全，但回调在Unity主线程
		/// </summary>
		public static TimerId InvokeAfter(double seconds, Action callback)
		{
			lock (timer)
			{
				return timer.InvokeAfter(seconds, callback);
			}
		}
		/// <summary>
		///     多线程安全，但回调在Unity主线程
		/// </summary>
		public static void InvokeAfter(ref TimerId id, double seconds, Action action)
		{
			timer.CancelInvoke(id);
			id = timer.InvokeAfter(seconds, action);
		}
		/// <summary>
		///     多线程安全
		/// </summary>
		public static void CancelInvoke(ref TimerId id) => timer.CancelInvoke(ref id);
		/// <summary>
		///     多线程安全，但回调在Unity主线程
		/// </summary>
		public static Awaitable Await(double seconds)
		{
			var awaitable = Awaitable.Create(out var handle);
			InvokeAfter(seconds, handle.Set);
			return awaitable;
		}
	}
}
