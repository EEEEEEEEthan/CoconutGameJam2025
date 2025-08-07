using System;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static float Remapped(this int @this, float from0, float to0, float from1, float to1) => (@this - from0) / (to0 - from0) * (to1 - from1) + from1;
		public static double Remapped(this int @this, double from1, double to1, double from2, double to2) => (@this - from1) / (to1 - from1) * (to2 - from2) + from2;
		public static bool Between(this int @this, int minIncluded, int maxExcluded) => @this >= minIncluded && @this < maxExcluded;
		public static bool Between(this int @this, int min, bool minIncluded, int max, bool maxIncluded) =>
			(minIncluded ? @this >= min : @this > min) && (maxIncluded ? @this <= max : @this < max);
		public static int Clamped(this int @this, int min, int max) => @this < min ? min : @this > max ? max : @this;
		public static int Clamped(this int @this, Range range) => @this < range.Start.Value ? range.Start.Value : @this > range.End.Value ? range.End.Value : @this;
		public static byte ClampedAsByte(this int @this) => @this < byte.MinValue ? byte.MinValue : @this > byte.MaxValue ? byte.MaxValue : (byte)@this;
		public static ushort ClampedAsUInt16(this int @this) =>
			@this < ushort.MinValue ? ushort.MinValue :
			@this > ushort.MaxValue ? ushort.MaxValue : (ushort)@this;
	}
}
