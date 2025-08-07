using System.Collections.Generic;
using Game.Utilities.Pools;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static Pooled ToArray<T>(this List<T> @this, out T[] array)
		{
			var count = @this.Count;
			var disposable = ArrayPoolThreaded<T>.RentWithoutDefaultValue(count, out array);
			@this.CopyTo(array, 0);
			return disposable;
		}
		public static bool TryRandomPop<T>(this List<T> @this, out T result)
		{
			if (@this is null || @this.Count == 0)
			{
				result = default;
				return false;
			}
			var index = random.Next(0, @this.Count);
			result = @this[index];
			@this.RemoveAtSwapLast(index);
			return true;
		}
	}
}
