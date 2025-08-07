using System;
using System.Collections.Generic;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static Pooled ToList<T>(this IEnumerable<T> @this, out List<T> list)
		{
			var disposable = ListPoolThreaded<T>.Rent(out list);
			foreach (var item in @this) list.Add(item);
			return disposable;
		}
		public static bool TryRandomPick<T>(this IEnumerable<KeyValuePair<T, float>> @this, out T result)
		{
			var value = float.MinValue;
			result = default;
			var hasItem = false;
			foreach (var (item, weight) in @this)
			{
				if (weight <= 0) continue;
				hasItem = true;
				var score = Mathf.Pow(random.NextSingle(), 1 / weight);
				if (score > value)
				{
					result = item;
					value = score;
				}
			}
			return hasItem;
		}
		public static T RandomPickOrDefault<T>(this IEnumerable<T> @this, Func<T, float> weightGetter, T defaultValue)
		{
			if (!@this.TryRandomPick(weightGetter, out var result)) return defaultValue;
			return result;
		}
		public static bool TryRandomPick<T>(this IEnumerable<T> @this, Func<T, float> weightGetter, out T result)
		{
			var value = float.MinValue;
			result = default;
			var hasItem = false;
			foreach (var item in @this)
			{
				hasItem = true;
				float weight;
				try
				{
					weight = weightGetter(item);
					if (weight <= 0) continue;
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					continue;
				}
				var score = Mathf.Pow(random.NextSingle(), 1 / weight);
				if (score > value)
				{
					result = item;
					value = score;
				}
			}
			return hasItem;
		}
		public static T RandomPickOrDefault<T>(this IEnumerable<KeyValuePair<T, float>> @this, T defaultValue)
		{
			if (!@this.TryRandomPick(out T result)) return defaultValue;
			return result;
		}
		public static bool TryRandomPick<T>(this IEnumerable<T> @this, out T result)
		{
			var value = float.MinValue;
			result = default;
			var hasItem = false;
			foreach (var item in @this)
			{
				hasItem = true;
				var score = random.NextSingle();
				if (score > value)
				{
					value = score;
					result = item;
				}
			}
			return hasItem;
		}
		public static T RandomPick<T>(this IEnumerable<T> @this)
		{
			if (!@this.TryRandomPick(out var result)) throw new InvalidOperationException("Sequence contains no elements.");
			return result;
		}
		public static T RandomPickOrDefault<T>(this IEnumerable<T> @this, T defaultValue = default)
		{
			if (!@this.TryRandomPick(out var result)) return defaultValue;
			return result;
		}
		public static Vector2 Center(this IEnumerable<Vector2> @this)
		{
			var center = Vector2.zero;
			var count = 0;
			foreach (var pos in @this)
			{
				center += pos;
				count += 1;
			}
			center /= count;
			return center;
		}
		public static Vector2 Center(this IEnumerable<Vector2Int> @this)
		{
			var center = Vector2.zero;
			var count = 0;
			foreach (var pos in @this)
			{
				center += pos;
				count += 1;
			}
			center /= count;
			return center;
		}
	}
}
