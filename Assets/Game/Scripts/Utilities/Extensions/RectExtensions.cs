using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static Rect Expanded(this Rect @this, float amount) => new(@this.x - amount, @this.y - amount, @this.width + amount + amount, @this.height + amount + amount);
	}
}
