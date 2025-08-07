using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
namespace Game.Utilities.UnityTools.Attributes
{
	/// <inheritdoc />
	/// <summary>
	///     为UnityEngine.Object类型的字段添加一个按标签筛选资产的按钮
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class FilterByLabelAttribute : PropertyAttribute
	{
#if UNITY_EDITOR
		[CustomPropertyDrawer(typeof(FilterByLabelAttribute))]
		sealed class FilterByLabelAttributeDrawer : PropertyDrawer
		{
			static void SetupObjectSelector(Object obj, Type type, bool allowSceneObjects, string filter, int controlID)
			{
				var method = typeof(EditorGUIUtility).GetMethod("SetupObjectSelector", (BindingFlags)0xffff);
				Debug.Assert(method != null, nameof(method) + " != null");
				method.Invoke(null, new object[] { obj, type, allowSceneObjects, filter, controlID, });
			}
			static Type GetType(SerializedProperty property)
			{
				var ownerType = property.serializedObject.targetObject.GetType();
				var fieldInfo = GetFieldInfoFromProperty(property, ownerType);
				return fieldInfo?.FieldType ?? typeof(Object);
			}
			static FieldInfo GetFieldInfoFromProperty(SerializedProperty property, [NotNull] Type ownerType)
			{
				var propertyPath = property.propertyPath;
				var fieldNames = propertyPath.Split('.');
				FieldInfo fieldInfo = null;
				var currentType = ownerType;
				foreach (var fieldName in fieldNames)
					// 处理数组情况
					if (fieldName.Contains("["))
					{
						var arrayFieldName = fieldName[..fieldName.IndexOf("[", StringComparison.Ordinal)];
						fieldInfo = currentType.GetField(arrayFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						if (fieldInfo == null) return null;
						if (fieldInfo.FieldType.IsArray)
							currentType = fieldInfo.FieldType.GetElementType();
						else // 可能是List<T>类型
							currentType = fieldInfo.FieldType.GetGenericArguments()[0];
						Assert.IsNotNull(currentType);
					}
					else
					{
						fieldInfo = currentType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						if (fieldInfo == null) return null;
						currentType = fieldInfo.FieldType;
					}
				return fieldInfo;
			}
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				if (property.propertyType != SerializedPropertyType.ObjectReference)
				{
					EditorGUI.PropertyField(position, property, label);
					return;
				}
				const float buttonWidth = 25;
				var objectFieldRect = new Rect(position.x, position.y, position.width - buttonWidth - 2, position.height);
				var buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, position.height);
				EditorGUI.PropertyField(objectFieldRect, property, label);
				if (GUI.Button(buttonRect, "F"))
				{
					var attribute = (FilterByLabelAttribute)this.attribute;
					var type = GetType(property);
					if (type.IsSubclassOf(typeof(MonoBehaviour))) type = typeof(GameObject);
					showObjectPicker(property.objectReferenceValue, type, false, $"l:{attribute.label}");
					static void showObjectPicker(Object obj, Type type, bool allowSceneObjects, string filter)
					{
						if (Event.current?.commandName == "ObjectSelectorClosed")
							EditorApplication.delayCall += () => SetupObjectSelector(obj, type, allowSceneObjects, filter, 0);
						else
							SetupObjectSelector(obj, type, allowSceneObjects, filter, 0);
					}
				}
				if (Event.current.commandName == "ObjectSelectorUpdated")
				{
					var selected = EditorGUIUtility.GetObjectPickerObject();
					Undo.RecordObject(property.serializedObject.targetObject, "Set Property Value");
					property.objectReferenceValue = selected;
					property.serializedObject.ApplyModifiedProperties();
				}
			}
		}
#endif
		/// <summary>
		///     用于筛选的标签
		/// </summary>
		readonly string label;
		/// <summary>
		///     创建一个按标签筛选资产的Attribute
		/// </summary>
		/// <param name="label">用于筛选的标签</param>
		public FilterByLabelAttribute(string label) => this.label = label;
	}
}
