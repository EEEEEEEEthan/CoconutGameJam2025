using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CeilToInt(this float value) => (int)Math.Ceiling(value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FloorToInt(this float value) => (int)Math.Floor(value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RoundToInt(this float value) => (int)Math.Round(value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Remapped(this float value, float from0, float to0, float from1, float to1)
		{
			if (to0 == from0)
			{
				Debug.LogError("Remapped: to1 == from1");
				return from1;
			}
			return from1 + (value - from0) * (to1 - from1) / (to0 - from0);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamped(this float value, float min, float max) => Math.Min(Math.Max(value, min), max);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamped(this float value, (float min, float max) range) => Math.Min(Math.Max(value, range.min), range.max);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Between(this float value, float minIncluded, float maxExculded) => value >= minIncluded && value < maxExculded;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Between(this float value, Vector2 range) => value.Between(range.x, range.y);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Powered(this float value, float power) => Mathf.Pow(value, power);
	}
}
