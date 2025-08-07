using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CeilToInt(this double value) => (int)Math.Ceiling(value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FloorToInt(this double value) => (int)Math.Floor(value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RoundToInt(this double value) => (int)Math.Round(value);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Remapped(this double value, double from0, double to0, double from1, double to1)
		{
			if (to0 == from0)
			{
				Debug.LogError("Remapped: to1 == from1");
				return from1;
			}
			return from1 + (value - from0) * (to1 - from1) / (to0 - from0);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Clamped(this double value, double min, double max) => Math.Min(Math.Max(value, min), max);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Between(this double value, double minIncluded, double maxExculded) => value >= minIncluded && value < maxExculded;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Between(this double value, Vector2 range) => value.Between(range.x, range.y);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Powered(this double value, double power) => Math.Pow(value, power);
	}
}
