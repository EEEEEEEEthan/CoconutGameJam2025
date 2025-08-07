namespace Game.Utilities.Pools
{
	public static class StopwatchPool
	{
		public static Pooled Rent(out System.Diagnostics.Stopwatch stopwatch)
		{
			var pool = new ObjectPool<System.Diagnostics.Stopwatch>(
				create: () => new(),
				onRent: obj =>
				{
					obj.Restart();
					return true;
				},
				onReturn: null,
				onDispose: null
			);
			return pool.Rent(out stopwatch);
		}
	}
}
