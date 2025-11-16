#if UNITY_EDITOR
using UnityEngine;
namespace Helpers
{
	public class ShowIfAttribute : PropertyAttribute
	{
		public string ConditionFieldName { get; }
		public int EnumValue { get; }

		public ShowIfAttribute(string conditionFieldName, int enumValue)
		{
			ConditionFieldName = conditionFieldName;
			EnumValue = enumValue;
		}
	}
}
#endif
