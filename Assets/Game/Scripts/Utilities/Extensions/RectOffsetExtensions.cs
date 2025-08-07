using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static RectOffset Copy(this RectOffset @this) =>
			new()
			{
				left = @this.left, right = @this.right, top = @this.top, bottom = @this.bottom,
			};
	}
}
