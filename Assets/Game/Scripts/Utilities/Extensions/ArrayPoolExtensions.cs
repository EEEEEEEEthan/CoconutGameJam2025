using System;
using System.Buffers;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public struct ArrayPoolDisposable<T> : IDisposable
		{
			internal ArrayPool<T> pool;
			internal T[] array;
			public void Dispose()
			{
				if (array is { })
				{
					pool.Return(array);
					pool = null;
					array = null;
				}
			}
		}
		public static ArrayPoolDisposable<T> Rent<T>(this ArrayPool<T> pool, int length, out T[] array)
		{
			array = pool.Rent(length);
			return new() { pool = pool, array = array, };
		}
	}
}
