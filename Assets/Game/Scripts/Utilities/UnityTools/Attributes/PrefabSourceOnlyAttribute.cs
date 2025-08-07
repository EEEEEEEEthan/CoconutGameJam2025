using System;
using UnityEditor;
using UnityEngine;
namespace Game.Utilities.UnityTools.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class PrefabSourceOnlyAttribute : PropertyAttribute
	{
#if UNITY_EDITOR
		[CustomPropertyDrawer(typeof(PrefabSourceOnlyAttribute))]
		sealed class DisableInPrefabDrawer : PropertyDrawer
		{
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				EditorGUI.BeginDisabledGroup(IsDisabled(property));
				EditorGUI.PropertyField(position, property, label, true);
				EditorGUI.EndDisabledGroup();
			}
			bool IsDisabled(SerializedProperty property)
			{
				if (property.serializedObject.targetObject is not Component component) return false;
				if (!PrefabUtility.IsAnyPrefabInstanceRoot(component.gameObject)) return false;
				var addedComponents = PrefabUtility.GetAddedComponents(component.gameObject);
				foreach (var obj in addedComponents)
					if (obj.instanceComponent == component)
						return false;
				return true;
			}
		}
#endif
	}
}
