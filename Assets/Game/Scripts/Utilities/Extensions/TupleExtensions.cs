using System;
using System.Collections;
using System.Collections.Generic;
using Game.Utilities.Pools;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public struct TupleEnumerator<T> : IEnumerator<T>
		{
			Pooled disposable;
			List<T>.Enumerator enumerator;
			T IEnumerator<T>.Current => enumerator.Current;
			object IEnumerator.Current => enumerator.Current;
			public TupleEnumerator((T, T) tuple)
			{
				disposable = ListPoolThreaded<T>.Rent(out var list);
				list.Add(tuple.Item1);
				list.Add(tuple.Item2);
				enumerator = list.GetEnumerator();
			}
			public TupleEnumerator((T, T, T) tuple)
			{
				disposable = ListPoolThreaded<T>.Rent(out var list);
				list.Add(tuple.Item1);
				list.Add(tuple.Item2);
				list.Add(tuple.Item3);
				enumerator = list.GetEnumerator();
			}
			public TupleEnumerator((T, T, T, T) tuple)
			{
				disposable = ListPoolThreaded<T>.Rent(out var list);
				list.Add(tuple.Item1);
				list.Add(tuple.Item2);
				list.Add(tuple.Item3);
				list.Add(tuple.Item4);
				enumerator = list.GetEnumerator();
			}
			public TupleEnumerator((T, T, T, T, T) tuple)
			{
				disposable = ListPoolThreaded<T>.Rent(out var list);
				list.Add(tuple.Item1);
				list.Add(tuple.Item2);
				list.Add(tuple.Item3);
				list.Add(tuple.Item4);
				list.Add(tuple.Item5);
				enumerator = list.GetEnumerator();
			}
			public TupleEnumerator((T, T, T, T, T, T) tuple)
			{
				disposable = ListPoolThreaded<T>.Rent(out var list);
				list.Add(tuple.Item1);
				list.Add(tuple.Item2);
				list.Add(tuple.Item3);
				list.Add(tuple.Item4);
				list.Add(tuple.Item5);
				list.Add(tuple.Item6);
				enumerator = list.GetEnumerator();
			}
			public TupleEnumerator((T, T, T, T, T, T, T) tuple)
			{
				disposable = ListPoolThreaded<T>.Rent(out var list);
				list.Add(tuple.Item1);
				list.Add(tuple.Item2);
				list.Add(tuple.Item3);
				list.Add(tuple.Item4);
				list.Add(tuple.Item5);
				list.Add(tuple.Item6);
				list.Add(tuple.Item7);
				enumerator = list.GetEnumerator();
			}
			void IEnumerator.Reset() => enumerator.MoveNext();
			void IDisposable.Dispose()
			{
				disposable.Dispose();
				disposable = default;
				enumerator.Dispose();
			}
			bool IEnumerator.MoveNext() => enumerator.MoveNext();
		}
		public static TupleEnumerator<T> GetEnumerator<T>(this (T, T) tuple) => new(tuple);
		public static TupleEnumerator<T> GetEnumerator<T>(this (T, T, T) tuple) => new(tuple);
		public static TupleEnumerator<T> GetEnumerator<T>(this (T, T, T, T) tuple) => new(tuple);
		public static TupleEnumerator<T> GetEnumerator<T>(this (T, T, T, T, T) tuple) => new(tuple);
		public static TupleEnumerator<T> GetEnumerator<T>(this (T, T, T, T, T, T) tuple) => new(tuple);
		public static TupleEnumerator<T> GetEnumerator<T>(this (T, T, T, T, T, T, T) tuple) => new(tuple);
		public static T RandomPick<T>(this (T, T) tuple) =>
			random.Next(0, 2) switch
			{
				0 => tuple.Item1,
				1 => tuple.Item2,
				_ => throw new ArgumentOutOfRangeException(),
			};
		public static T RandomPick<T>(this (T, T, T) tuple) =>
			random.Next(0, 3) switch
			{
				0 => tuple.Item1,
				1 => tuple.Item2,
				2 => tuple.Item3,
				_ => throw new ArgumentOutOfRangeException(),
			};
		public static T RandomPick<T>(this (T, T, T, T) tuple) =>
			random.Next(0, 4) switch
			{
				0 => tuple.Item1,
				1 => tuple.Item2,
				2 => tuple.Item3,
				3 => tuple.Item4,
				_ => throw new ArgumentOutOfRangeException(),
			};
		public static T RandomPick<T>(this (T, T, T, T, T) tuple) =>
			random.Next(0, 5) switch
			{
				0 => tuple.Item1,
				1 => tuple.Item2,
				2 => tuple.Item3,
				3 => tuple.Item4,
				4 => tuple.Item5,
				_ => throw new ArgumentOutOfRangeException(),
			};
		public static T RandomPick<T>(this (T, T, T, T, T, T) tuple) =>
			random.Next(0, 6) switch
			{
				0 => tuple.Item1,
				1 => tuple.Item2,
				2 => tuple.Item3,
				3 => tuple.Item4,
				4 => tuple.Item5,
				5 => tuple.Item6,
				_ => throw new ArgumentOutOfRangeException(),
			};
		public static T RandomPick<T>(this (T, T, T, T, T, T, T) tuple) =>
			random.Next(0, 7) switch
			{
				0 => tuple.Item1,
				1 => tuple.Item2,
				2 => tuple.Item3,
				3 => tuple.Item4,
				4 => tuple.Item5,
				5 => tuple.Item6,
				6 => tuple.Item7,
				_ => throw new ArgumentOutOfRangeException(),
			};
		public static T RandomPick<T>(this (T, T, T, T, T, T, T, T) tuple) =>
			random.Next(0, 8) switch
			{
				0 => tuple.Item1,
				1 => tuple.Item2,
				2 => tuple.Item3,
				3 => tuple.Item4,
				4 => tuple.Item5,
				5 => tuple.Item6,
				6 => tuple.Item7,
				7 => tuple.Item8,
				_ => throw new ArgumentOutOfRangeException(),
			};
		public static T RandomPick<T>(this (T, T, T, T, T, T, T, T, T) tuple) =>
			random.Next(0, 9) switch
			{
				0 => tuple.Item1,
				1 => tuple.Item2,
				2 => tuple.Item3,
				3 => tuple.Item4,
				4 => tuple.Item5,
				5 => tuple.Item6,
				6 => tuple.Item7,
				7 => tuple.Item8,
				8 => tuple.Item9,
				_ => throw new ArgumentOutOfRangeException(),
			};
		public static T RandomPick<T>(this (T, T, T, T, T, T, T, T, T, T) tuple) =>
			random.Next(0, 10) switch
			{
				0 => tuple.Item1,
				1 => tuple.Item2,
				2 => tuple.Item3,
				3 => tuple.Item4,
				4 => tuple.Item5,
				5 => tuple.Item6,
				6 => tuple.Item7,
				7 => tuple.Item8,
				8 => tuple.Item9,
				9 => tuple.Item10,
				_ => throw new ArgumentOutOfRangeException(),
			};
		public static T RandomPick<T>(this (T, T, T, T, T, T, T, T, T, T, T) tuple) =>
			random.Next(0, 11) switch
			{
				0 => tuple.Item1,
				1 => tuple.Item2,
				2 => tuple.Item3,
				3 => tuple.Item4,
				4 => tuple.Item5,
				5 => tuple.Item6,
				6 => tuple.Item7,
				7 => tuple.Item8,
				8 => tuple.Item9,
				9 => tuple.Item10,
				10 => tuple.Item11,
				_ => throw new ArgumentOutOfRangeException(),
			};
	}
}
