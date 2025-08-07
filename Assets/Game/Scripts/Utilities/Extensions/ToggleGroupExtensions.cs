using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		static FieldInfo toggleGroupFieldToggles;
		public static IReadOnlyList<Toggle> GetToggles(this ToggleGroup @this)
		{
			try
			{
				if (toggleGroupFieldToggles is null)
				{
					var type = typeof(ToggleGroup);
					toggleGroupFieldToggles = type.GetField("m_Toggles", BindingFlags.NonPublic | BindingFlags.Instance);
				}
				// ReSharper disable once PossibleNullReferenceException
				return (IReadOnlyList<Toggle>)toggleGroupFieldToggles.GetValue(@this);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				return Array.Empty<Toggle>();
			}
		}
	}
}
