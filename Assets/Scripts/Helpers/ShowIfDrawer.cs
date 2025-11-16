#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace Helpers
{
	[CustomPropertyDrawer(typeof(ShowIfAttribute))]
	public class ShowIfDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			ShowIfAttribute showIf = (ShowIfAttribute)attribute;
			SerializedProperty conditionProp = property.serializedObject.FindProperty(showIf.ConditionFieldName);

			if (ShouldShow(conditionProp, showIf))
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			ShowIfAttribute showIf = (ShowIfAttribute)attribute;
			SerializedProperty conditionProp = property.serializedObject.FindProperty(showIf.ConditionFieldName);

			return ShouldShow(conditionProp, showIf) ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
		}

		private bool ShouldShow(SerializedProperty conditionProp, ShowIfAttribute showIf)
		{
			if (conditionProp == null) return false;

			if (conditionProp.propertyType == SerializedPropertyType.Enum)
			{
				return conditionProp.enumValueIndex == showIf.EnumValue;
			}

			Debug.LogWarning("ShowIf only supports enums in this version.");
			return false;
		}
	}
}

#endif
