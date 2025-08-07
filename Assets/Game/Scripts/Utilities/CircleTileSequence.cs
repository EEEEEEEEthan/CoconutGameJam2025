using System;
using System.Collections;
using System.Collections.Generic;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Utilities
{
	public static class CircleTileSequence
	{
		public struct PositionEnumerator : IEnumerable<Vector2Int>, IEnumerator<Vector2Int>
		{
			readonly Vector2Int center;
			readonly float minSqrDistance;
			readonly float maxSqrDistance;
			int index;
			public Vector2Int Current => Get(index - 1) + center;
			object IEnumerator.Current => Current;
			public PositionEnumerator(Vector2Int center, float minDistance, float maxDistance)
			{
				index = 0;
				this.center = center;
				minSqrDistance = minDistance * minDistance;
				maxSqrDistance = maxDistance * maxDistance;
			}
			public PositionEnumerator GetEnumerator() => this;
			public bool MoveNext()
			{
				while (true)
				{
					++index;
					var sqrDistance = (Current - center).sqrMagnitude;
					if (sqrDistance >= maxSqrDistance) return false;
					if (sqrDistance >= minSqrDistance) return true;
				}
			}
			IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator() => this;
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			void IEnumerator.Reset() => index = 0;
			void IDisposable.Dispose() { }
		}
		static readonly IEnumerator<Vector2Int> generator = Generator();
		static readonly List<Vector2Int> cache = new();
		public static Vector2Int Get(int index)
		{
			while (cache.Count <= index)
			{
				generator.MoveNext();
				cache.Add(generator.Current);
			}
			return cache[index];
		}
		static IEnumerator<Vector2Int> Generator()
		{
			using (HeapSinglePoolThreaded<Vector2Int>.Rent(out var heap))
			{
				heap.Add(Vector2Int.zero, 0);
				for (var radius = 1;; radius++)
				{
					var threshold = radius * radius;
					while (heap.TryPeek(out var pos, out var value) && value < threshold)
					{
						heap.Pop();
						yield return pos;
					}
					for (var x = -radius; x <= radius; x++)
					{
						var sqrDistance = x * x + radius * radius;
						heap.Add(Vector2IntExtensions.Create(x, -radius), sqrDistance);
						heap.Add(Vector2IntExtensions.Create(x, radius), sqrDistance);
					}
					for (var y = -radius + 1; y < radius; y++)
					{
						var sqrDistance = radius * radius + y * y;
						heap.Add(Vector2IntExtensions.Create(-radius, y), sqrDistance);
						heap.Add(Vector2IntExtensions.Create(radius, y), sqrDistance);
					}
				}
			}
			// ReSharper disable once IteratorNeverReturns
		}
	}
}
