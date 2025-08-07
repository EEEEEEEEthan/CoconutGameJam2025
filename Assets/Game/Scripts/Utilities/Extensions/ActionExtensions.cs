using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
[assembly: SuppressMessage("Unity.PerformanceCriticalCodeInvocation", "Debug.LogException")]
namespace Game.Utilities
{
	public static partial class Extensions
	{
		[SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TryInvoke(this Action @this)
		{
			try
			{
				@this.Invoke();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		[SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TryInvoke<T>(this Action<T> @this, T arg)
		{
			try
			{
				@this.Invoke(arg);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TryInvoke<T1, T2>(this Action<T1, T2> @this, T1 arg1, T2 arg2)
		{
			try
			{
				@this.Invoke(arg1, arg2);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TryInvoke<T1, T2, T3>(this Action<T1, T2, T3> @this, T1 arg1, T2 arg2, T3 arg3)
		{
			try
			{
				@this.Invoke(arg1, arg2, arg3);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ICallbackItem ToCallbackItem(this Action @this) => new CallbackItem(@this);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ICallbackItem<T> ToCallbackItem<T>(this Action<T> @this) => new CallbackItem<T>(@this);
	}
}
