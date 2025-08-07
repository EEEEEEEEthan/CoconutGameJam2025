using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Game.Utilities
{
	public interface ICallbackItem
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Invoke();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void TryInvoke()
		{
			try
			{
				Invoke();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
	public interface ICallbackItem<in T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Invoke(T arg);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void TryInvoke(T arg)
		{
			try
			{
				Invoke(arg);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
