using System.Collections.Generic;
namespace Game.Utilities.Pools
{
	public static class StackPoolThreaded<T>
	{
		static readonly ObjectPool<Stack<T>> threadedPool = new(
			create: () => new(),
			onReturn: obj =>
			{
				obj.Clear();
				return true;
			},
			onRent: null,
			onDispose: null
		);
		public static Pooled Rent(out Stack<T> stack) => threadedPool.Rent(out stack);
	}
}
