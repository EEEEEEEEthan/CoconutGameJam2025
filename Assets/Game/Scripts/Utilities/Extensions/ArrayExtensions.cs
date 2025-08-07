using System;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static void MemSet<T>(this T[] array, T value, int start, int length)
		{
			var trueLength = array.Length;
			if (start < 0 || start >= trueLength) throw new ArgumentOutOfRangeException(nameof(start), start, $"Start index is out of [{0}, {array.Length})");
			if (length < 0 || start + length > trueLength) throw new ArgumentOutOfRangeException(nameof(length), length, $"Length is out of [{0}, {array.Length - start})");
			if (length <= 0) return;
			if (value is null || value.Equals(null))
			{
				Array.Clear(array, start, length);
				return;
			}
			array[start] = value;
			var copied = 1;
			var remainingLength = length - 1;
			while (remainingLength > 0)
			{
				var copyLength = Math.Min(remainingLength, copied);
				Array.Copy(array, start, array, start + copied, copyLength);
				remainingLength -= copyLength;
				copied += copyLength;
			}
		}
		public static void MemSet<T>(this T[] array, T value) => MemSet(array, value, 0, array.Length);
		public static ArraySegment<T> Segment<T>(this T[] array, int start, int count) => new(array, start, count);
		public static void Shuffle<T>(this T[] @this)
		{
			var count = @this.Length;
			for (var i = 0; i < count; ++i)
			{
				var j = random.Next(i, count);
				(@this[i], @this[j]) = (@this[j], @this[i]);
			}
		}
		public static T GetValueOrDefault<T>(this T[] array, int index, T defaultValue = default) => index >= 0 && index < array.Length ? array[index] : defaultValue;
		public static T RandomPick<T>(this T[] array) => array[random.Next(0, array.Length)];
	}
}
