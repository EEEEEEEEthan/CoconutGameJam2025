using System.Runtime.CompilerServices;
using UnityEngine;
namespace Game.Utilities
{
	public static class GameMath
	{
		const int maxCachedSqrt = 65535;
		static readonly float[] sqrtFloats;
		static readonly ulong[] bitValues = new ulong[64];
		static GameMath()
		{
			sqrtFloats = new float[maxCachedSqrt];
			for (var i = 0; i < maxCachedSqrt; ++i) sqrtFloats[i] = Mathf.Sqrt(i);
			for (var i = 0; i < bitValues.Length; ++i) bitValues[i] = 1ul << i;
		}
		public static int GetBitIndex(ulong value)
		{
			if (!TryGetBitIndex(value, out var index)) Debug.LogError($"Value {value} is not a valid bit index.");
			return index;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetBitIndex(ulong value, out int index)
		{
			index = bitValues.BinarySearch(value, null);
			if (index >= 0) return true;
			index = ~index;
			return false;
		}
		public static float Sqrt(int value)
		{
			if (value < maxCachedSqrt) return sqrtFloats[value];
			return Mathf.Sqrt(value);
		}
		public static uint LinearCongruentialGenerator(uint seed, uint a, uint c, uint m) => (a * seed + c) % m;
		public static uint LinearCongruentialGenerator(uint seed, uint m) => LinearCongruentialGenerator(seed, 1664525, 1013904223, m);
		public static uint LinearCongruentialGenerator(uint seed, uint min, uint max) => LinearCongruentialGenerator(seed, max - min) + min;
		public static int LinearCongruentialGenerator(int seed, int a, int c, int m) => (int)LinearCongruentialGenerator((uint)seed, (uint)a, (uint)c, (uint)m);
		public static int LinearCongruentialGenerator(int seed, int m) => (int)LinearCongruentialGenerator((uint)seed, (uint)m);
		public static int LinearCongruentialGenerator(int seed, int min, int max) => (int)LinearCongruentialGenerator((uint)seed, (uint)min, (uint)max);
	}
}
