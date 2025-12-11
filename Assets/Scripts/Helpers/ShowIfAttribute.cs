using System;
using UnityEngine;

namespace Helpers
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ShowIfAttribute : PropertyAttribute
	{
		public string ConditionFieldName { get; }
		public int[] EnumValues { get; }

		public ShowIfAttribute(string conditionFieldName, params int[] enumValues)
		{
			ConditionFieldName = conditionFieldName;
			EnumValues = enumValues;
		}
	}
}
