using System;
using System.Collections.Generic;
using Game.Utilities.Pools;
namespace Game.Utilities
{
	public static class Enum<T> where T : Enum
	{
		static Dictionary<string, T> cachedNameToValue;
		static IReadOnlyList<T> values;
		// ReSharper disable once StaticMemberInGenericType
		static bool maxValueCached;
		static T maxValue;
		// ReSharper disable once StaticMemberInGenericType
		static int? maxBitPosition;
		public static IReadOnlyList<T> Values
		{
			get
			{
				if (values == null)
				{
					var rawValues = Enum.GetValues(typeof(T));
					var values = new T[rawValues.Length];
					Enum<T>.values = values;
					for (var i = 0; i < rawValues.Length; ++i) values[i] = (T)rawValues.GetValue(i);
				}
				return values;
			}
		}
		public static T MaxValue
		{
			get
			{
				if (!maxValueCached)
				{
					var got = false;
					var value = maxValue;
					foreach (var enumValue in Values)
						if (!got || Comparer<T>.Default.Compare(enumValue, value) > 0)
						{
							value = enumValue;
							got = true;
						}
					maxValue = value;
					maxValueCached = true;
				}
				return maxValue;
			}
		}
		public static int MaxBitPosition
		{
			get
			{
				if (maxBitPosition is null)
				{
					var value = 0;
					foreach (var enumValue in Values)
					{
						var enumValueInt = Convert.ToInt64(enumValue);
						var bitPosition = 0;
						while (enumValueInt > 0)
						{
							enumValueInt >>= 1;
							++bitPosition;
						}
						if (bitPosition > value) value = bitPosition;
					}
					maxBitPosition = value;
				}
				return maxBitPosition.Value;
			}
		}
		static Dictionary<string, T> NameToValue
		{
			get
			{
				if (cachedNameToValue is null)
				{
					cachedNameToValue = new();
					foreach (var enumValue in Values) cachedNameToValue[enumValue.ToString()] = enumValue;
				}
				return cachedNameToValue;
			}
		}
		public static string GetFlagText(T value)
		{
			var typeName = typeof(T).Name;
			var valueAsLong = Convert.ToInt64(value);
			if (valueAsLong == 0) return "0";
			using var _ = ListPoolThreaded<string>.Rent(out var builder);
			foreach (var enumValue in Values)
			{
				var enumValueAsLong = Convert.ToInt64(enumValue);
				if (enumValueAsLong == 0 || (valueAsLong & enumValueAsLong) != enumValueAsLong) continue;
				builder.Add($"{typeName}.{enumValue}");
			}
			return string.Join(" | ", builder);
		}
		public static T Parse(string text) => NameToValue[text];
		public static bool TryParse(string text, out T value) => NameToValue.TryGetValue(text, out value);
	}
}
