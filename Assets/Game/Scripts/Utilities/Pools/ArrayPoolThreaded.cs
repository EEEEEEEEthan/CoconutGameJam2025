using System.Collections.Generic;
namespace Game.Utilities.Pools
{
	public static class ArrayPoolThreaded<T>
	{
		static readonly List<ObjectPool<T[]>> pools = new();
		/// <summary>
		///     租借一个数据混乱的数组
		/// </summary>
		/// <param name="length"></param>
		/// <param name="array">装有未知数据</param>
		/// <returns></returns>
		public static Pooled RentWithoutDefaultValue(int length, out T[] array)
		{
			lock (pools)
			{
				if (length >= pools.Count)
					while (length >= pools.Count)
						pools.Add(null);
				pools[length] ??= CreatePool(length);
				var pooled = pools[length].Rent(out var item);
				array = item;
				return pooled;
			}
		}
		public static Pooled Rent(int length, out T[] array, T defaultValue)
		{
			var disposable = RentWithoutDefaultValue(length, out array);
			array.MemSet(defaultValue);
			return disposable;
		}
		static ObjectPool<T[]> CreatePool(int length) => new(create: () => new T[length]);
	}
}
