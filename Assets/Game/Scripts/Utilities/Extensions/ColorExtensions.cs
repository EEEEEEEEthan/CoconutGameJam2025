using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static Vector3 ToHSV(this Color color)
		{
			Color.RGBToHSV(color, out var h, out var s, out var v);
			return new(h, s, v);
		}
		public static Color WithAlpha(this Color color, float a) => new(color.r, color.g, color.b, a);
	}
}
