using System.Collections;
using System.Collections.Generic;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		readonly struct EnumerableConverter<T> : IEnumerable<T>
		{
			readonly IEnumerator<T> enumerator;
			public EnumerableConverter(IEnumerator<T> enumerator) => this.enumerator = enumerator;
			public IEnumerator<T> GetEnumerator() => enumerator;
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
		public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> @this) => new EnumerableConverter<T>(@this);
		public static IEnumerator<T> GetEnumerator<T>(this IEnumerator<T> @this) => @this;
		public static bool MoveNext<T>(this IEnumerator<T> @this, out T current)
		{
			if (@this.MoveNext())
			{
				current = @this.Current;
				return true;
			}
			current = default;
			return false;
		}
	}
}
