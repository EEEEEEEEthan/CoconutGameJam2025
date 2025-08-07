using System;
using Game.Utilities.Time;
namespace Game.Utilities.UnityTools
{
	/// <summary>
	///     多线程安全
	/// </summary>
#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoad]
#endif
	public static class EditorTimerManager
	{
		static readonly Timer timer;
		static readonly DateTime startTime;
		static EditorTimerManager()
		{
			timer = new();
#if UNITY_EDITOR
			UnityEditor.EditorApplication.update -= update;
			UnityEditor.EditorApplication.update += update;
#endif
			static void update() => timer.Update((DateTime.Now - startTime).TotalSeconds);
		}
		/// <summary>
		///     多线程安全，但回调在Unity主线程
		/// </summary>
		public static TimerId InvokeAfter(double seconds, Action callback)
		{
#if !UNITY_EDITOR
			return default;
#endif
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
#if !UNITY_EDITOR
			return;
#endif
			lock (timer)
			{
				timer.CancelInvoke(id);
				id = timer.InvokeAfter(seconds, action);
			}
		}
		/// <summary>
		///     多线程安全
		/// </summary>
		public static void CancelInvoke(ref TimerId id)
		{
#if !UNITY_EDITOR
			return;
#endif
			lock (timer)
			{
				timer.CancelInvoke(ref id);
			}
		}
		/// <summary>
		///     多线程安全，但回调在Unity主线程
		/// </summary>
		public static Awaitable Await(double seconds)
		{
#if !UNITY_EDITOR
			return default;
#endif
			lock (timer)
			{
				var awaitable = Awaitable.Create(out var handle);
				InvokeAfter(seconds, handle.Set);
				return awaitable;
			}
		}
	}
}
