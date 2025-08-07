using System;
using System.Collections.Generic;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		static readonly Random random = new();
		public static void RemoveAtSwapLast<T>(this IList<T> @this, int item)
		{
			var index = @this.Count - 1;
			@this[item] = @this[index];
			@this.RemoveAt(index);
		}
		public static bool RemoveSwapLast<T>(this IList<T> @this, T item)
		{
			var index = @this.IndexOf(item);
			if (index < 0) return false;
			@this.RemoveAtSwapLast(index);
			return true;
		}
		public static bool BinaryInsert<T>(this IList<T> @this, T value, bool allowsDuplicate = true, IComparer<T> comparer = null)
		{
			var index = @this.BinarySearch(value, comparer);
			if (index < 0)
				index = ~index;
			else if (!allowsDuplicate) return false;
			@this.Insert(index, value);
			return true;
		}
		public static bool BinaryRemove<T>(this IList<T> @this, T value, IComparer<T> comparer = null)
		{
			var index = @this.BinarySearch(value, comparer);
			if (index < 0) return false;
			@this.RemoveAt(index);
			return true;
		}
		public static int BinarySearch<T>(this IList<T> @this, T value, IComparer<T> comparer = null)
		{
			comparer ??= Comparer<T>.Default;
			var left = 0;
			var right = @this.Count - 1;
			while (left <= right)
			{
				var mid = left + (right - left) / 2;
				var midValue = @this[mid];
				var comparison = comparer.Compare(midValue, value);
				switch (comparison)
				{
					case 0:
						return mid;
					case < 0:
						left = mid + 1;
						break;
					default:
						right = mid - 1;
						break;
				}
			}
			return ~left;
		}
		public static int BinarySearch(this ulong[] @this, ulong value)
		{
			var left = 0;
			var right = @this.Length - 1;
			while (left <= right)
			{
				var mid = left + (right - left) / 2;
				var midValue = @this[mid];
				if (midValue == value) return mid;
				if (midValue < value)
					left = mid + 1;
				else
					right = mid - 1;
			}
			return ~left;
		}
		public static bool TryPopLast<T>(this IList<T> @this, out T item)
		{
			if (@this.Count <= 0)
			{
				item = default;
				return false;
			}
			item = @this[^1];
			@this.RemoveAt(@this.Count - 1);
			return true;
		}
	}
}
