using UnityEditor;
using UnityEngine;

namespace ProjectQuad
{
    [CustomPropertyDrawer(typeof(Vector2IntRangeAttribute))]
    public class Vector2IntRangeAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            Vector2Int value = EditorGUI.Vector2IntField(position, label, property.vector2IntValue);
            if (EditorGUI.EndChangeCheck())
            {
                var rangeAttribute = (Vector2IntRangeAttribute)attribute;
                value.x = Mathf.Clamp(value.x, rangeAttribute.minX, rangeAttribute.maxX);
                value.y = Mathf.Clamp(value.y, rangeAttribute.minY, rangeAttribute.maxY);
                property.vector2IntValue = value;
            }
        }
    }
}
