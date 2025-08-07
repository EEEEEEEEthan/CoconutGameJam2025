using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Utilities.Pools;
using UnityEngine.Assertions;
namespace Game.Utilities
{
	public static class Primes
	{
		static int[] primes =
		{
			2, 3, 5, 7, 11, 13, 17, 19,
		};
		public static int GetGreatestCommonDivisor(int a, int b)
		{
			// Calculate greatest common divisor using Euclidean algorithm
			while (b != 0)
			{
				var t = b;
				b = a % b;
				a = t;
			}
			return Math.Abs(a);
		}
		public static bool IsPrime(int n)
		{
			if (n < 2) return false;
			ExpandUntilGreatestGreaterOrEquals(n);
			var index = Array.BinarySearch(primes, n);
			return index >= 0;
		}
		public static int GetPrimeGreaterThan(int n)
		{
			if (n < 2) return 2;
			ExpandUntilGreatestGreaterThan(n);
			var index = Array.BinarySearch(primes, n);
			if (index >= 0)
				++index;
			else
				index = ~index;
			var result = primes[index];
			Assert.IsTrue(result > n && IsPrime(result));
			return result;
		}
		public static int GetPrimeGreaterOrEquals(int n)
		{
			if (n < 2) return 2;
			ExpandUntilGreatestGreaterThan(n);
			var index = Array.BinarySearch(primes, n);
			if (index < 0) index = ~index;
			var result = primes[index];
			Assert.IsTrue(result >= n && IsPrime(result));
			return result;
		}
		public static int GetPrimeLessThan(int n)
		{
			if (n < 2) throw new ArgumentOutOfRangeException(nameof(n), n, "n must be greater than or equal to 2.");
			ExpandUntilGreatestGreaterOrEquals(n);
			var index = Array.BinarySearch(primes, n);
			if (index >= 0)
				--index;
			else
				index = ~index - 1;
			var result = primes[index];
			Assert.IsTrue(result < n && IsPrime(result));
			return result;
		}
		public static int GetPrimeLessOrEquals(int n)
		{
			if (n < 2) throw new ArgumentOutOfRangeException(nameof(n), n, "n must be greater than or equal to 2.");
			ExpandUntilGreatestGreaterOrEquals(n);
			var index = Array.BinarySearch(primes, n);
			if (index < 0) index = ~index - 1;
			var result = primes[index];
			Assert.IsTrue(result <= n && IsPrime(result));
			return result;
		}
		public static IEnumerable<int> IterPrimeFactors(int n)
		{
			var sqrtN = (int)Math.Sqrt(n);
			var index = Array.BinarySearch(primes, sqrtN);
			if (index >= 0)
			{
				yield return n;
				yield break;
			}
			index = ~index - 1;
			for (var i = index; i-- > 0;)
			{
				var prime = primes[i];
				if (n % prime == 0) yield return prime;
			}
		}
		static void ExpandUntilGreatestGreaterThan(int n)
		{
			if (primes[^1] > n) return;
			using var _ = ListPoolThreaded<int>.Rent(out var list);
			list.AddRange(primes);
			while (true)
			{
				var value = FindPrimeGreaterThan(list[^1]);
				list.Add(value);
				if (value > n) break;
			}
			primes = list.ToArray();
		}
		static void ExpandUntilGreatestGreaterOrEquals(int n)
		{
			if (primes[^1] >= n) return;
			using var _ = ListPoolThreaded<int>.Rent(out var list);
			list.AddRange(primes);
			while (true)
			{
				var value = FindPrimeGreaterThan(list[^1]);
				list.Add(value);
				if (value >= n) break;
			}
			primes = list.ToArray();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int FindPrimeGreaterThan(int n)
		{
			// Find the next prime number greater than n
			var candidate = n + 1;
			while (!CheckPrime(candidate)) ++candidate;
			return candidate;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool CheckPrime(int n)
		{
			var sqrt = (int)Math.Sqrt(n);
			for (var i = 2; i <= sqrt; i++)
				if (n % i == 0)
					return false;
			return true;
		}
	}
}
