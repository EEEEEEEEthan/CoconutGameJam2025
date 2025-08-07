using System.Collections.Generic;
namespace Game.Utilities.Pools
{
	public static class HashSetPoolThreaded<T>
	{
		static readonly ObjectPool<HashSet<T>> threadedPool = new(
			create: () => new(),
			onReturn: obj =>
			{
				obj.Clear();
				return true;
			},
			onRent: null,
			onDispose: null
		);
		public static Pooled Rent(out HashSet<T> set) => threadedPool.Rent(out set);
	}
}
