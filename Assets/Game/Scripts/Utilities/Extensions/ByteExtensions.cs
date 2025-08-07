namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static float Remapped(this byte @this, float from0, float to0, float from1, float to1) => (@this - from0) / (to0 - from0) * (to1 - from1) + from1;
		public static double Remapped(this byte @this, double from1, double to1, double from2, double to2) => (@this - from1) / (to1 - from1) * (to2 - from2) + from2;
		public static bool Between(this byte @this, byte minIncluded, byte maxExcluded) => @this >= minIncluded && @this < maxExcluded;
		public static bool Between(this byte @this, byte min, bool minIncluded, byte max, bool maxIncluded) =>
			(minIncluded ? @this >= min : @this > min) && (maxIncluded ? @this <= max : @this < max);
	}
}
