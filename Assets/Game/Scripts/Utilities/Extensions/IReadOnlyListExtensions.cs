using System.Collections.Generic;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static T RandomPick<T>(this IReadOnlyList<T> @this) => @this[random.Next(0, @this.Count)];
		public static bool TryRandomPick<T>(this IReadOnlyList<T> @this, out T result)
		{
			if (@this.Count == 0)
			{
				result = default;
				return false;
			}
			result = @this[random.Next(0, @this.Count)];
			return true;
		}
	}
}
