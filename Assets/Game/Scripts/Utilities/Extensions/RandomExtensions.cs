using System;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static float NextSingle(this Random @this) => (float)@this.NextDouble();
		public static float NextSingle(this Random @this, float min, float max) => @this.NextSingle().Remapped(0, 1, min, max);
	}
}
