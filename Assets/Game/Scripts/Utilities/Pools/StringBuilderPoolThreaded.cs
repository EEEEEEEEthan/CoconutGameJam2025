using System.Text;
namespace Game.Utilities.Pools
{
	public static class StringBuilderPoolThreaded
	{
		static readonly ObjectPool<StringBuilder> pool = new(
			create: () => new(),
			onReturn: obj =>
			{
				obj.Clear();
				return true;
			},
			onRent: null,
			onDispose: null
		);
		public static Pooled Rent(out StringBuilder builder) => pool.Rent(out builder);
	}
}
