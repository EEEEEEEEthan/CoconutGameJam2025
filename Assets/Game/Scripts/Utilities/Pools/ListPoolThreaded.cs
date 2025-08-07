using System.Collections.Generic;
namespace Game.Utilities.Pools
{
	public static class ListPoolThreaded<T>
	{
		static readonly ObjectPool<List<T>> threadedPool = new(
			create: () => new(),
			onReturn: obj =>
			{
				obj.Clear();
				return true;
			},
			onRent: null,
			onDispose: null
		);
		public static Pooled Rent(IEnumerable<T> data, out List<T> list)
		{
			var disposable = threadedPool.Rent(out list);
			foreach (var d in data) list.Add(d);
			return disposable;
		}
		public static Pooled Rent(out List<T> list) => threadedPool.Rent(out list);
	}
}
