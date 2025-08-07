using System.Collections.Generic;
namespace Game.Utilities.Pools
{
	public static class DictionaryPoolThreaded<TKey, TValue>
	{
		static readonly ObjectPool<Dictionary<TKey, TValue>> threadedPool = new(
			create: () => new(),
			onReturn: obj =>
			{
				obj.Clear();
				return true;
			},
			onRent: null,
			onDispose: null
		);
		public static Pooled Rent(out Dictionary<TKey, TValue> list) => threadedPool.Rent(out list);
	}
}
