namespace Game.Utilities
{
	public partial class Extensions
	{
		public static bool GetBit(this ulong @this, int index) => (@this & (1UL << index)) != 0;
		public static void SetBit(ref this ulong @this, int index, bool value)
		{
			if (value)
				@this |= 1UL << index;
			else
				@this &= ~(1UL << index);
		}
	}
}
