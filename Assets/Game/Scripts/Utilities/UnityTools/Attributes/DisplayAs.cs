using System;
using UnityEngine;
namespace Game.Utilities.UnityTools.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class DisplayAsAttribute : PropertyAttribute
	{
#if UNITY_EDITOR
		[UnityEditor.CustomPropertyDrawer(typeof(DisplayAsAttribute))]
		sealed class DisplayAsAttributeDrawer : UnityEditor.PropertyDrawer
		{
			public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
			{
				var attribute = (DisplayAsAttribute)this.attribute;
				if (attribute.ShowCodeName) label.text = $"{attribute.Name}({property.name})";
				UnityEditor.EditorGUI.PropertyField(position, property, label, true);
			}
		}
#endif
		internal string Name { get; }
		internal bool ShowCodeName { get; }
		public DisplayAsAttribute(string name, bool showCodeName = true)
		{
			Name = name;
			ShowCodeName = showCodeName;
		}
	}
}
