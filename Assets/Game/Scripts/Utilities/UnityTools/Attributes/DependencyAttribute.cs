using System;
using UnityEditor;
using UnityEngine;
namespace Game.Utilities.UnityTools.Attributes
{
	/// <inheritdoc />
	/// <summary>
	///     仅在依赖的字段不为空(或不为零)时才显示
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class DependencyAttribute : PropertyAttribute
	{
#if UNITY_EDITOR
		[CustomPropertyDrawer(typeof(DependencyAttribute))]
		sealed class DependencyDrawer : PropertyDrawer
		{
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				if (Disabled(property)) return 0;
				return base.GetPropertyHeight(property, label);
			}
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				if (Disabled(property)) return;
				EditorGUI.PropertyField(position, property, label, true);
			}
			bool Disabled(SerializedProperty property)
			{
				var dependencyAttribute = (DependencyAttribute)attribute;
				var dependencyProperty = property.serializedObject.FindProperty(dependencyAttribute.dependencyName);
				if (dependencyProperty is null) return false;
				return dependencyProperty.propertyType switch
				{
					SerializedPropertyType.Integer => dependencyProperty.intValue == 0,
					SerializedPropertyType.Boolean => !dependencyProperty.boolValue,
					SerializedPropertyType.Float => dependencyProperty.floatValue == 0,
					SerializedPropertyType.Enum => dependencyProperty.enumValueIndex == 0,
					SerializedPropertyType.Generic => false,
					SerializedPropertyType.String => dependencyProperty.stringValue.IsNullOrEmpty(),
					SerializedPropertyType.Color => dependencyProperty.colorValue == default,
					SerializedPropertyType.ObjectReference => !dependencyProperty.objectReferenceValue,
					SerializedPropertyType.LayerMask => dependencyProperty.intValue == 0,
					SerializedPropertyType.Vector2 => dependencyProperty.vector2Value == default,
					SerializedPropertyType.Vector3 => dependencyProperty.vector3Value == default,
					SerializedPropertyType.Vector4 => dependencyProperty.vector4Value == default,
					SerializedPropertyType.Rect => dependencyProperty.rectValue == default,
					SerializedPropertyType.ArraySize => dependencyProperty.intValue == 0,
					SerializedPropertyType.Character => dependencyProperty.intValue == 0,
					SerializedPropertyType.AnimationCurve => dependencyProperty.animationCurveValue == null,
					SerializedPropertyType.Bounds => dependencyProperty.boundsValue == default,
					SerializedPropertyType.Gradient => dependencyProperty.gradientValue == null,
					SerializedPropertyType.Quaternion => dependencyProperty.quaternionValue == default,
					SerializedPropertyType.ExposedReference => dependencyProperty.exposedReferenceValue == null,
					SerializedPropertyType.FixedBufferSize => dependencyProperty.intValue == 0,
					SerializedPropertyType.Vector2Int => dependencyProperty.vector2IntValue == default,
					SerializedPropertyType.Vector3Int => dependencyProperty.vector3IntValue == default,
					SerializedPropertyType.RectInt => dependencyProperty.rectIntValue == default,
					SerializedPropertyType.BoundsInt => dependencyProperty.boundsIntValue == default,
					SerializedPropertyType.ManagedReference => dependencyProperty.managedReferenceValue == null,
					SerializedPropertyType.Hash128 => dependencyProperty.hash128Value == default,
					_ => false,
				};
			}
		}
#endif
		public readonly string dependencyName;
		public DependencyAttribute(string dependencyName) => this.dependencyName = dependencyName;
	}
}
