using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T TryInvoke<T>(this Func<T> func)
		{
			try
			{
				return func();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				return default;
			}
		}
	}
}
