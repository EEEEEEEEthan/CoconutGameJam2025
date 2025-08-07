using Game.Utilities.Collections;
namespace Game.Utilities.Pools
{
	public static class HeapSinglePoolThreaded<TKey>
	{
		static readonly ObjectPool<HeapSingle<TKey>> threadedPool = new(
			create: () => new(),
			onReturn: obj =>
			{
				obj.Clear();
				return true;
			},
			onRent: null,
			onDispose: null
		);
		public static Pooled Rent(out HeapSingle<TKey> heap) => threadedPool.Rent(out heap);
	}
}
